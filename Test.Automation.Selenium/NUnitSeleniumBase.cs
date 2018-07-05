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
            if (BrowserSettings == null)
            {
                throw new ApplicationException("No '<browserSettings>' section found in App.config file.\r\nEnsure the App.config file exists and it contains a '<browserSettings>' section.");
            }

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
            //var logs = Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.log"); // NUnit

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
            if (!DriverService.IsRunning)
            {
                // DriverService should be started in StartTestRun().
                throw new ApplicationException($"The DriverService '{DriverService}' is not running.");
            }

            Driver = WebDriverFactory.CreateWebDriver(BrowserSettings, DriverService);
            if (BrowserSettings.DeleteAllCookies)
            {
                Driver.Manage().Cookies.DeleteAllCookies();
            }
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
                {"Run Environment", TestRunSetting.ToUpper() },
                {"Base URI", Settings.BaseUri.AbsoluteUri }
            };
            WriteLogToOutput("Environment Settings", environment);
        }

        private void LogWebDriverServiceState()
        {
            var webDriver = new Dictionary<string, string>
            {
                {"Service Name", DriverService.ToString() },
                {"IsRunning", DriverService.IsRunning.ToString().ToUpperInvariant() },
                {"Cmd Window", DriverService.HideCommandPromptWindow
                ? "HIDDEN"
                : "VISIBLE" },
                {"Service URI", DriverService.ServiceUrl.AbsoluteUri },
                {"Service Host", DriverService.HostName },
                {"Service Port", DriverService.Port.ToString() },
                {"Process ID", DriverService.ProcessId.ToString() },
            };
            WriteLogToOutput("WebDriver Service State", webDriver);
        }

        private void LogWebDriverBrowserState()
        {

            var browser = new Dictionary<string, string>
            {
                {"Browser Setting", BrowserSettings.Name.ToString() },
                {"Browser Name", Driver?.Capabilities.BrowserName.ToString() },
                {"Browser Version", Driver?.Capabilities.Version == string.Empty
                ? "-NO DATA-"
                : Driver?.Capabilities.Version},
                {"DefaultTimeout", BrowserSettings.DefaultWaitTimeout.ToString() + " s." }
            };

            // Browser specific settings.
            switch (BrowserSettings.Name)
            {
                case DriverType.IE:
                    browser.Add("InitialBrowserUrl", BrowserSettings.InitialBrowserUrl == string.Empty
                        ? "Default initial start page for the WebDriver server."
                        : BrowserSettings.InitialBrowserUrl);
                    browser.Add("IgnoreProtectedMode", BrowserSettings.IgnoreProtectedModeSettings.ToString().ToUpperInvariant());
                    browser.Add("EnsureCleanSession", BrowserSettings.EnsureCleanSession.ToString().ToUpperInvariant());
                    if (BrowserSettings.IsMaximized)
                    {
                        browser.Add("IsMaximized", BrowserSettings.IsMaximized.ToString().ToUpperInvariant());
                    }
                    else
                    {
                        browser.Add("Browser Size", BrowserSettings.Size.ToString());
                        browser.Add("Browser Position", BrowserSettings.Position.ToString());
                    }
                    browser.Add("VerboseLogging", BrowserSettings.EnableVerboseLogging.ToString().ToUpperInvariant());
                    break;
                case DriverType.Chrome:
                    browser.Add("InitialBrowserUrl", BrowserSettings.InitialBrowserUrl == string.Empty
                        ? "data:,"
                        : BrowserSettings.InitialBrowserUrl);
                    browser.Add("IsHeadless", BrowserSettings.IsHeadless.ToString().ToUpperInvariant());
                    if (!BrowserSettings.IsHeadless)
                    {
                        if (BrowserSettings.IsMaximized)
                        {
                            browser.Add("IsMaximized", BrowserSettings.IsMaximized.ToString().ToUpperInvariant());
                        }
                        else
                        {
                            browser.Add("Browser Size", BrowserSettings.Size.ToString());
                            browser.Add("Browser Position", BrowserSettings.Position.ToString());
                        }
                    }
                    else
                    {
                        // Force start-maximized in headless mode.
                        browser.Add("IsMaximized", "TRUE");
                    }
                    browser.Add("DwnldDefaultDir", BrowserSettings.DownloadDefaultDir == string.Empty
                        ? "-NO DATA-"
                        : BrowserSettings.DownloadDefaultDir);
                    browser.Add("VerboseLogging", BrowserSettings.EnableVerboseLogging.ToString().ToUpperInvariant());
                    browser.Add("LogLevel", BrowserSettings.LogLevel.ToString());
                    break;
                case DriverType.MicrosoftEdge:
                    browser.Add("InitialBrowserUrl", BrowserSettings.InitialBrowserUrl == string.Empty
                        ? "about:blank"
                        : BrowserSettings.InitialBrowserUrl);
                    if (BrowserSettings.IsMaximized)
                    {
                        browser.Add("IsMaximized", BrowserSettings.IsMaximized.ToString().ToUpperInvariant());
                    }
                    else
                    {
                        browser.Add("Browser Size", BrowserSettings.Size.ToString());
                        browser.Add("Browser Position", BrowserSettings.Position.ToString());
                    }
                    browser.Add("VerboseLogging", BrowserSettings.EnableVerboseLogging.ToString().ToUpperInvariant());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BrowserSettings.Name), BrowserSettings.Name, "Invalid argument for browser name - does not match DriverType.");
            }

            WriteLogToOutput("WebDriver Instance Settings", browser);

            var browserState = new Dictionary<string, string>
            {
                {"Browser Caps", ((IHasCapabilities) Driver)?.Capabilities.ToString()},
                {"Browser URL", Driver?.Url},
                {"Browser Title", Driver?.Title}
            };

            WriteLogToOutput("Browser End State", browserState);
        }

        private void LogWebDriverLogs()
        {
            if (Driver == null)
            {
                return;
            }

            switch (Driver.Capabilities.BrowserName)
            {
                /******************************************************************
                 * The IE driver does not support getting logs of any kind.
                 * Those commands have not been implemented.
                 * They will be implemented only when the server-side API is properly specified.
                 *
                 * The MicrosoftEdge driver throws an 'Unknown Command' exception.
                 * ***************************************************************/

                case "internet explorer":
                case "MicrosoftEdge":
                    if(Debugger.IsAttached)
                    {
                        Console.WriteLine($"WebDriver instance logs not available for: '{Driver.Capabilities.BrowserName}'.");
                    }
                    return;
                default:
                    break;
            }

            var logTypes = default(IEnumerable<string>);

            try
            {
                var logs = Driver.Manage().Logs;
                logTypes = logs.AvailableLogTypes;
            }
            catch (Exception ex)
            {
                // Trap any exceptions if the webdriver does not support instance logs.
                Console.WriteLine($"WebDriver instance logs not available for: '{Driver.Capabilities.BrowserName}'; EXCEPTION: {ex.Message}");
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
                    RenameServiceLog(DriverType.Chrome.ToDescription());
                    break;
                case DriverType.IE:
                    RenameServiceLog(DriverType.IE.ToDescription());
                    break;
                case DriverType.MicrosoftEdge:
                    RenameServiceLog(DriverType.MicrosoftEdge.ToDescription());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BrowserSettings.Name), BrowserSettings.Name, "Invalid argument for browser name - does not match DriverType.");
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
                if (Debugger.IsAttached)
                {
                    Console.WriteLine($"DriverService log not available for: '{browserName}'.");
                }
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
            // BUG: Chrome headless mode does not support Driver.Manage().Window.
            if (BrowserSettings.Name == DriverType.Chrome && BrowserSettings.IsHeadless)
            {
                return;
            }
            
            var currentWindow = Driver.Manage().Window;

            if (BrowserSettings.IsMaximized)
            {
                currentWindow.Maximize();
            }
            else
            {
                if (!currentWindow.Position.Equals(BrowserSettings.Position))
                {
                    currentWindow.Position = BrowserSettings.Position;
                }

                if (!currentWindow.Size.Equals(BrowserSettings.Size))
                {
                    currentWindow.Size = BrowserSettings.Size;
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
