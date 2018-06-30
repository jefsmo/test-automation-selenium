using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Test.Automation.Base;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents an instance of the SeleniumContext class. 
    /// This class contains the methods used to create a Selenium WebDriver for testing.
    /// </summary>
    public abstract class NUnitSeleniumBase : NUnitTestBase
    {
        private DriverService DriverService { get; set; }

        /// <summary>
        /// The WebDriver instance created for each test.
        /// </summary>
        protected RemoteWebDriver Driver { get; set; }

        #region APP CONFIG SETTINGS
        private static string TestRunSetting => ConfigurationManager.AppSettings["testRunSetting"];

        /// <summary>
        /// BrowserSettings section data from App.config.
        /// </summary>
        private static BrowserSettings BrowserSettings => ConfigurationManager.GetSection("browserSettings/" + TestRunSetting) as BrowserSettings;

        /// <summary>
        /// EnvironmentSettings section data from App.config.
        /// </summary>
        protected static EnvironmentSettings Settings
            => ConfigurationManager.GetSection("environmentSettings/" + TestRunSetting) as EnvironmentSettings;
        #endregion

        #region TEST ATTRIBUTED METHODS

        /// <summary>
        /// Initializes a single DriverService used by all the tests.
        /// </summary>
        /// <param name="binariesDir">The directory that contains the binaries used by the test when the test is run.</param>
        [OneTimeSetUp]
        public void StartTestRun()
        {
            // Only a single Service instance is started for the entire test run.
            DriverService = WebDriverFactory.CreateDriverService(BrowserSettings, TestContext.CurrentContext.TestDirectory);
            DriverService.Start();
        }

        /// <summary>
        /// Closes the DriverService and deletes DriverService logs.
        /// </summary>
        /// <param name="binariesDir">The directory that contains the binaries used by the test when the test is run.</param>
        [OneTimeTearDown]
        public void StopTestRun()
        {
            DriverService.Dispose();

            // Delete the log created by the DriverService in the binaries directory, it has been copied to the logs directory.
            //var logs = Directory.GetFiles(".", "*.log");    // VsTest
            //var logs = Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.log"); //NUnit

            var logs = Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.log");

            if (logs.Length.Equals(0))
            {
                return;
            }

            foreach (var log in logs)
            {
                try
                {
                    File.Delete(log);
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine(ioEx);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the WebDriver for each test.
        /// Applies WebDriver settings from App.config.
        /// </summary>
        [SetUp]
        public void StartTest()
        {
            Driver = WebDriverFactory.CreateWebDriver(BrowserSettings, DriverService);
            Driver.Manage().Cookies.DeleteAllCookies();
            SetBrowserWindow();
        }

        /// <summary>
        /// Closes the WebDriver instance and logs test information if the test fails or is run in debug mode.
        /// </summary>
        /// <param name="testAutomationContext">The current test data from the test framework test context.</param>
        [TearDown]
        public void StopTest()
        {
            // Log only if test status is not 'Passed' or if debugger is attached.
            if (!TestContext.CurrentContext.Result.Outcome.Status.Equals(TestStatus.Passed) || Debugger.IsAttached)
            {
                // Always log screenshot and test data if test fails or test is run in debug mode.
                LogTestEnvironment();
                LogWebDriverServiceState();
                LogWebDriverBrowserState();
                TakeScreenshot();

                // Only save WebDriver logs when test is run in debug mode.
                if (Debugger.IsAttached)
                {
                    //LogPageSource();
                    LogWebDriverLogs();
                    LogDriverServiceLog();
                }
            }

            Driver.Quit();
            Driver?.Dispose();
        }

        #endregion

        #region PRIVATE METHODS

        private void LogTestEnvironment()
        {
            // Log test evironment info.
            var environment = new Dictionary<string, string>
            {
                {"Run Environment", TestRunSetting.ToUpper()},
                {"Base URI", Settings.BaseUri.AbsoluteUri}
            };
            WriteLogToOutput("Environment Settings", environment);
        }

        private void LogWebDriverServiceState()
        {
            var webDriver = new Dictionary<string, string>
            {
                {"Service Name", DriverService.ToString()},
                {"Service State", DriverService.IsRunning ? "RUNNING" : "STOPPED"},
                {"Service Window", DriverService.HideCommandPromptWindow ? "HIDDEN" : "VISIBLE"},
                {"Service URI", DriverService.ServiceUrl.AbsoluteUri},
                {"Service Port", DriverService.Port.ToString()},
                {"Process ID", DriverService.ProcessId.ToString()}
            };
            WriteLogToOutput("WebDriver Service Settings", webDriver);
        }

        private void LogWebDriverBrowserState()
        {

            var browser = new Dictionary<string, string>
            {
                { "Browser Name", BrowserSettings.Name.ToString() },
                { "Browser Window", BrowserSettings.IsMaximized ? "MAXIMIZED" : "NORMAL" },
                { "Browser Mode", BrowserSettings.IsHeadless ? "HEADLESS" : "NORMAL" }
            };

            if (!BrowserSettings.IsMaximized)
            {
                browser.Add("Browser Size", BrowserSettings.Size.ToString());
                browser.Add("Browser Position", BrowserSettings.Position.ToString());
            }
            WriteLogToOutput("WebDriver Browser Settings", browser);
            
            if (Driver == null) return;

            var browserState = new Dictionary<string, string>
            {
                {"Browser Caps", ((IHasCapabilities) Driver).Capabilities.ToString()},
                {"Browser URL", Driver.Url},
                {"Browser Title", Driver.Title}
            };
            WriteLogToOutput("Browser State", browserState);
        }

        private void LogWebDriverLogs()
        {
            if (Driver == null)
            {
                return;
            }

            var logTypes = default(IEnumerable<string>);

            //  The IE driver does not support getting logs of any kind.
            //  Those commands have not been implemented.
            //  They will be implemented only when the server-side API is properly specified.
            try
            {
                var logs = Driver.Manage().Logs;
                logTypes = logs.AvailableLogTypes;
            }
            catch (NullReferenceException nullEx)
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine($"WebDriver Logs not available for '{Driver.Capabilities.BrowserName}' WebDriver; EXCEPTION: {nullEx.Message}");
                }
                return;
            }

            foreach (var logType in logTypes)
            {
                var logName = RemoveInvalidFileNameChars(TestContext.CurrentContext.Test.Name) + "." + logType.ToUpper() +  (logType == "har" ? ".har" : ".md");

                var logPath = Path.Combine(TestContext.CurrentContext.TestDirectory, logName);
                SaveLogType(logType, logPath);
            }
        }

        private void SaveLogType(string logType, string logPath)
        {
            var driverLog = Driver.Manage().Logs.GetLog(logType);
            var sb = new StringBuilder();

            foreach (var logEntry in driverLog)
            {
                sb.Append(logEntry);
            }

            if (sb.Length < 1) return;

            //Console.WriteLine($"Saving log type '{logType.ToUpper()}'");

            File.WriteAllText(logPath, sb.ToString());
            AttachFileToOutputWindow(logPath);
        }

        private void LogDriverServiceLog()
        {
            switch (BrowserSettings.Name)
            {
                case DriverType.Chrome:
                    RenameServiceLog("chromedriver");
                    break;
                case DriverType.IE:
                    RenameServiceLog("IEDriverServer");
                    break;
                case DriverType.PhantomJs:
                    RenameServiceLog("phantomjsdriver");
                    break;
                case DriverType.Edge:
                    RenameServiceLog("Edge");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BrowserSettings.Name), (int)BrowserSettings.Name, null);
            }
        }

        private void RenameServiceLog(string browserName)
        {
            var sourceName = browserName + ".log";
            var destName = RemoveInvalidFileNameChars(TestContext.CurrentContext.Test.Name) + "." + browserName.ToUpper() + ".md";
            var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, sourceName);
            var destPath = Path.Combine(TestContext.CurrentContext.TestDirectory, destName);

            if (!File.Exists(sourcePath))
            {
                return;
            }

            try
            {
                File.Copy(sourcePath, destPath, true);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine(ioEx);
            }

            AttachFileToOutputWindow(destPath);
        }

        private void TakeScreenshot()
        {
            if (Driver is null)
            {
                return;
            }

            var ssFileName = RemoveInvalidFileNameChars(TestContext.CurrentContext.Test.Name) + ".png";
            var ssFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, ssFileName);

            // Take a screenshot.
            var ss = Driver.GetScreenshot();

            if (string.IsNullOrEmpty(ss.ToString()))
            {
                Console.WriteLine(
                    "Screenshot Not Taken: A webpage error may be preventing the screenshot from being taken.\r\n" +
                    "Repro the test manually to identify the webpage error.");
            }

            SaveScreenshot(ss, ssFilePath);
        }

        private static void SaveScreenshot(Screenshot screenshot, string screenshotFile)
        {
            // Save the screenshot to a file, overwriting the file if it already exits.
            try
            {
                screenshot.SaveAsFile(screenshotFile, ScreenshotImageFormat.Png);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                Console.WriteLine("File Not Saved: An error occurred attempting to save the file.");
                Console.WriteLine(ex);
            }

            AttachFileToOutputWindow(screenshotFile);
        }

        private void LogPageSource()
        {
            var pageSourceData = string.Empty;

            if (Driver == null)
            {
                return;
            }

            // Get page source.
            pageSourceData = Driver.PageSource;

            if (string.IsNullOrEmpty(pageSourceData))
            {
                Console.WriteLine("PageSource Not Available: The WebDriver did not contain any PageSource data.");
                return;
            }

            var psFileName = RemoveInvalidFileNameChars(TestContext.CurrentContext.Test.Name) + ".html";
            var psFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, psFileName);

            SavePageSource(pageSourceData, psFilePath);
        }

        private static void SavePageSource(string pageSourceData, string pageSourceFile)
        {
            try
            {
                // Save the page source to a file, if the target file already exists, it is overwritten.
                File.WriteAllText(pageSourceFile, pageSourceData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Not Saved: An error occurred attempting to save the file.");
                Console.WriteLine(ex);
            }

            AttachFileToOutputWindow(pageSourceFile);
        }

        private void SetBrowserWindow()
        {
            var currentWindow = Driver.Manage().Window;

            if (BrowserSettings.IsMaximized)
            {
                Driver.Manage().Window.Maximize();
            }
            else
            {
                if (!currentWindow.Position.Equals(BrowserSettings.Position))
                {
                    Driver.Manage().Window.Position = BrowserSettings.Position;
                }

                if (!currentWindow.Size.Equals(BrowserSettings.Size))
                {
                    Driver.Manage().Window.Size = BrowserSettings.Size;
                }
            }
        }

        /// <summary>
        /// Attaches the file to the Visual Studio output window in the Attachments section.
        /// </summary>
        /// <param name="fileToAttach">The full path and file name of the file to attach.</param>
        private static void AttachFileToOutputWindow(string fileToAttach)
        {
            TestContext.AddTestAttachment(fileToAttach);
        }

        private static void WriteFilePathToTestOutput(string file)
        {
            if (File.Exists(file))
            {
                // How to create a file URI.
                var uri = new UriBuilder
                {
                    Scheme = "file",    // The Internet access 'file' protocol.
                    Host = GetFqdn(),   // A DNS-style domain name or IP address.
                    Port = -1,          // If the portNumber is set to a value of -1, this indicates that the default port value for the scheme will be used to connect to the host.
                    Path =  file        // The path to the Internet resource.
                }
                .Uri
                .AbsoluteUri;

                ////Console.WriteLine(uri);
                Console.WriteLine(file);
            }
            else
            {
                Console.WriteLine("File Not Found: An error occurred attempting to log the file.");
            }
        }

        private static string GetFqdn()
        {
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();

            if (domainName.Equals(string.Empty)) return hostName; // return the fully qualified name

            domainName = "." + domainName;

            if (!hostName.EndsWith(domainName)) // if hostname does not already include domain name
            {
                hostName += domainName;         // add the domain name part
            }

            return hostName;                    // return the fully qualified name
        }

        #endregion
    }
}
