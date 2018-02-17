using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using OpenQA.Selenium;
using Test.Automation.Base;
using Test.Automation.Selenium.Enums;
using Test.Automation.Selenium.Factories;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents an instance of the SeleniumContext class. 
    /// This class contains the methods used to create a Selenium WebDriver for testing.
    /// </summary>
    public sealed class SeleniumContext
    {
        private DriverService DriverService { get; set; }

        /// <summary>
        /// The WebDriver instance created for each test.
        /// </summary>
        public IWebDriver Driver { get; set; }

        /// <summary>
        /// The screenshot taken on test failure or when debugging.
        /// Screenshots are saved to the test output.
        /// </summary>
        public string Screenshot { get; private set; }

        #region APP CONFIG SETTINGS
        private string TestRunSetting
            => ConfigurationManager.AppSettings["testRunSetting"];

        /// <summary>
        /// BrowserSettings section data from App.config.
        /// </summary>
        private BrowserSettings BrowserSettings 
            => ConfigurationManager.GetSection("browserSettings/" + TestRunSetting) as BrowserSettings;

        /// <summary>
        /// EnvironmentSettings section data from App.config.
        /// </summary>
        public EnvironmentSettings EnvironmentSettings
            => ConfigurationManager.GetSection("environmentSettings/" + TestRunSetting) as EnvironmentSettings;
        #endregion
        
        #region TEST ATTRIBUTED METHODS

        /// <summary>
        /// Initializes a single DriverService used by all the tests.
        /// </summary>
        /// <param name="binariesDir">The directory that contains the binaries used by the test when the test is run.</param>
        public void StartTestRun(string binariesDir)
        {
            // Only a single Service instance is started for the entire test run.
            DriverService = DriverServiceFactory.CreateDriverService(BrowserSettings, binariesDir);
            DriverService.Start();
        }

        /// <summary>
        /// Closes the DriverService and deletes DriverService logs.
        /// </summary>
        /// <param name="binariesDir">The directory that contains the binaries used by the test when the test is run.</param>
        public  void StopTestRun(string binariesDir)
        {
            DriverService.Dispose();

            // Delete the log created by the DriverService in the binaries directory, it has been copied to the logs directory.
            ////var logs = Directory.GetFiles(".", "*.log");    // VsTest
            ////var logs = Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.log"); //NUnit

            var logs = Directory.GetFiles(binariesDir, "*.log");          
            if (logs.Length.Equals(0)) return;

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
        public void StopTest(ITestAutomationContext testAutomationContext)
        {
            // Log only if test status is not 'Passed' or if debugger is attached.
            if (!testAutomationContext.TestResultStatus.Equals(TestResultStatus.Pass) || Debugger.IsAttached)
            {
                // Always log screenshot and test data if test fails or test is run in debug mode.
                //TestAutomationBase.LogTestAttributes(testAutomationContext);
                //TestAutomationBase.LogTestContext(testAutomationContext);
                LogTestEnvironment(testAutomationContext);
                LogWebDriverServiceState(testAutomationContext);
                LogWebDriverBrowserState(testAutomationContext);
                Screenshot = LogScreenshot(testAutomationContext);

                // Only save WebDriver logs when test is run in debug mode.
                if (Debugger.IsAttached)
                {
                    LogPageSource(testAutomationContext);
                    LogWebDriverLogs(testAutomationContext);
                    LogDriverServiceLog(testAutomationContext);
                }
            }
            Driver.Quit();
        }

        #endregion

        #region PRIVATE METHODS

        private void LogTestEnvironment(ITestAutomationContext genericTestContext)
        {
            // Log test evironment info.
            var environment = new Dictionary<string, string>
            {
                {"Run Environment", TestRunSetting.ToUpper()},
                {"Base URI", EnvironmentSettings.BaseUri.AbsoluteUri}
            };
            NUnitTestBase.WriteLogToOutput("Environment Settings", environment);
        }

        private void LogWebDriverServiceState(ITestAutomationContext genericTestContext)
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
            NUnitTestBase.WriteLogToOutput("WebDriver Service Settings", webDriver);
        }

        private void LogWebDriverBrowserState(ITestAutomationContext genericTestContext)
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
            NUnitTestBase.WriteLogToOutput("WebDriver Browser Settings", browser);
            
            if (Driver == null) return;

            var browserState = new Dictionary<string, string>
            {
                {"Browser Caps", ((IHasCapabilities) Driver).Capabilities.ToString()},
                {"Browser URL", Driver.Url},
                {"Browser Title", Driver.Title}
            };
            NUnitTestBase.WriteLogToOutput("Browser State", browserState);
        }

        private void LogWebDriverLogs(ITestAutomationContext genericTestContext)
        {
            if (Driver == null) return;

            var logTypes = default(IEnumerable<string>);

            try
            {
                var logs = Driver.Manage().Logs;
                logTypes = logs.AvailableLogTypes;
            }
            catch (WebDriverException wdEx)
            {
                Console.WriteLine("Logs not available for this WebDriver.");
                Console.WriteLine(wdEx);
            }

            if (logTypes == null) return;

            foreach (var logType in logTypes)
            {
                var logName = genericTestContext.SafeTestName + "." + logType.ToUpper() +  (logType == "har" ? ".har" : ".md");

                var logPath = Path.Combine(genericTestContext.LogDirectory, logName);
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

            Console.WriteLine($"Saving log type '{logType.ToUpper()}'");

            File.WriteAllText(logPath, sb.ToString());
            WriteFilePathToTestOutput(logPath);
        }

        private void LogDriverServiceLog(ITestAutomationContext genericTestContext)
        {
            switch (BrowserSettings.Name)
            {
                case DriverType.Chrome:
                    RenameServiceLog(genericTestContext, "chromedriver");
                    break;
                case DriverType.Ie:
                    RenameServiceLog(genericTestContext, "IEDriverServer");
                    break;
                case DriverType.PhantomJs:
                    RenameServiceLog(genericTestContext, "phantomjsdriver");
                    break;
                case DriverType.Edge:
                    RenameServiceLog(genericTestContext, "Edge");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BrowserSettings.Name), (int)BrowserSettings.Name, null);
            }
        }

        private void RenameServiceLog(ITestAutomationContext genericTestContext, string browserName)
        {
            var sourceName = browserName + ".log";
            var destName = genericTestContext.SafeTestName + "." + browserName.ToUpper() + ".md";
            var source = Path.Combine(genericTestContext.TestBinariesDirectory, sourceName);
            var dest = Path.Combine(genericTestContext.LogDirectory, destName);

            if (!File.Exists(source)) return;

            try
            {
                File.Copy(source, dest, true);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine(ioEx);
            }

            WriteFilePathToTestOutput(dest);
        }

        private string LogScreenshot(ITestAutomationContext genericTestContext)
        {
            if (Driver == null) return null;

            var ssFileName = genericTestContext.SafeTestName + ".png";
            var ssFilePath = Path.Combine(genericTestContext.LogDirectory, ssFileName);

            // Take a screenshot.
            var ss = ((ITakesScreenshot)Driver).GetScreenshot();

            if (string.IsNullOrEmpty(ss.ToString()))
            {
                Console.WriteLine(
                    "Screenshot Not Taken: A webpage error may be preventing the screenshot from being taken.\r\n" +
                    "Repro the test manually to identify the webpage error.");
                return null;
            }

            var ssFile = SaveScreenshot(ss, ssFilePath);
            WriteFilePathToTestOutput(ssFile);
            return ssFile;
        }

        private void LogPageSource(ITestAutomationContext genericTestContext)
        {
            if (Driver == null) return;

            var psFileName = genericTestContext.SafeTestName + ".html";
            var psFilePath = Path.Combine(genericTestContext.LogDirectory, psFileName);

            // Get page source.
            var ps = Driver.PageSource;

            if (string.IsNullOrEmpty(ps))
            {
                Console.WriteLine("PageSource Not Available: The WebDriver did not contain any PageSource data.");
                return;
            }

            var psFile = SavePageSource(ps, psFilePath);
            WriteFilePathToTestOutput(psFile);
        }

        private static string SaveScreenshot(Screenshot screenshot, string file)
        {
            // Save the screenshot to a file, overwriting the file if it already exits.
            try
            {
                screenshot.SaveAsFile(file, ScreenshotImageFormat.Png);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                Console.WriteLine("File Not Saved: An error occurred attempting to save the file.");
                Console.WriteLine(ex);
            }

            return file;
        }

        private static string SavePageSource(string pageSource, string file)
        {
            try
            {
                // Save the page source to a file, if the target file already exists, it is overwritten.
                File.WriteAllText(file, pageSource, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Not Saved: An error occurred attempting to save the file.");
                Console.WriteLine(ex);
            }

            return file;
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

        private static void WriteFilePathToTestOutput(string file)
        {
            if (File.Exists(file))
            {
                // How to create a file URI.
                // ReSharper disable once UnusedVariable
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
