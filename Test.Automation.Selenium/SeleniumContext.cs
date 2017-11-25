using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using OpenQA.Selenium;
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
        private static DriverService Service { get; set; }

        private static string TestRunSetting => ConfigurationManager.AppSettings["testRunSetting"];

        /// <summary>
        /// BrowserSettings section data from App.config.
        /// </summary>
        private static BrowserSettings BrowserSettings 
            => ConfigurationManager.GetSection("browserSettings/" + TestRunSetting) as BrowserSettings;

        /// <summary>
        /// EnvironmentSettings section data from App.config.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static EnvironmentSettings EnvironmentSettings
            => ConfigurationManager.GetSection("environmentSettings/" + TestRunSetting) as EnvironmentSettings;

        /// <summary>
        /// The WebDriver instance created for each test.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public IWebDriver Driver { get; set; }

        /// <summary>
        /// The screenshot taken on test failure or when debugging.
        /// Screenshots are saved to the test output.
        /// </summary>
        public string Screenshot { get; private set; }

        #region TEST ATTRIBUTED METHODS

        /// <summary>
        /// Initializes a single DriverService used by all the tests.
        /// </summary>
        /// <param name="binariesDir"></param>
        public static void StartTestRun(string binariesDir)
        {
            // Only a single Service instance is started for the entire test run.
            Service = DriverServiceFactory.CreateDriverService(BrowserSettings, binariesDir);
            Service.Start();
        }

        /// <summary>
        /// Closes the DriverService and deletes DriverService logs.
        /// </summary>
        /// <param name="binariesDir"></param>
        public static void StopTestRun(string binariesDir)
        {
            Service?.Dispose();

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
            Driver = WebDriverFactory.CreateWebDriver(BrowserSettings, Service);
            Driver.Manage().Cookies.DeleteAllCookies();
            SetBrowserWindow();
        }

        /// <summary>
        /// Closes the WebDriver instance and logs test information if the test fails or is run in debug mode.
        /// </summary>
        /// <param name="testData">The current test data from the test context.</param>
        public void StopTest(TestData testData)
        {
            // Log only if test status is not 'Passed' or if debugger is attached.
            if (!testData.TestContext.TestResultStatus.Equals(TestResultStatus.Pass) || Debugger.IsAttached)
            {
                // Workdirectory: The directory to be used for outputting files created by this test run.
                LogTestInfo(testData);
                Screenshot = LogScreenshot(testData);

                // Save any WebDriver logs generated when test is run in debug mode.
                if (Debugger.IsAttached)
                {
                    LogPageSource(testData);
                    LogWebDriverLogs(testData);
                    LogDriverServiceLog(testData);
                }
            }

            Driver?.Quit();
        }
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Writes test repro info to the output window.
        /// </summary>
        private void LogTestInfo(TestData testData)
        {
            var attributes = new Dictionary<string,string>
            {
                {
                    "Owner",
                    testData.TestAttributes.Owner ?? "-no owner-"
                },
                {
                    "Description",
                    testData.TestAttributes.Description ?? "-no description-"
                },
                {
                    "Timeout", testData.TestAttributes.Timeout == int.MaxValue 
                    ? "Infinite" 
                    : testData.TestAttributes.Timeout / 60000 + " (seconds)"
                },
                {
                    "Test Priority",
                    testData.TestAttributes.Priority.ToString()
                },
                {
                    "Test Category", string.Join(", ", testData.TestAttributes.TestCategories.Count == 0
                    ? new List<string>
                    {
                        "-no test category-"
                    }
                    : testData.TestAttributes.TestCategories)
                },
                {
                    "Test Property",  string.Join(", ",testData.TestAttributes.TestProperties.Count == 0
                    ? new List<KeyValuePair<string,string>>
                    {
                        new KeyValuePair<string, string>("-no key-", "-no value-")
                    }
                    : testData.TestAttributes.TestProperties)
                },
                {
                    "Work Item", string.Join(", ", testData.TestAttributes.WorkItems.Count == 0
                    ? new List<string>
                    {
                        "-no work item-"
                    }
                    : testData.TestAttributes.WorkItems.Select(x => x.ToString()))
                }
            };

            WriteLogToOutput("Test Attributes", attributes);

            // Log test info.
            var environment = new Dictionary<string, string>
            {
                {"Run Environment", TestRunSetting.ToUpper()},
                {"Current Dir", Directory.GetCurrentDirectory() },
                {"Logs Dir", testData.TestContext.LogDirectory},
                {"Base URI", EnvironmentSettings.BaseUri.AbsoluteUri}
            };

            WriteLogToOutput("Environment Settings", environment);

            var webDriver = new Dictionary<string, string>
            {
                {"Service Name", Service.ToString()},
                {"Service State", Service.IsRunning ? "RUNNING" : "STOPPED"},
                {"Service Window", Service.HideCommandPromptWindow ? "HIDDEN" : "VISIBLE"},
                {"Service URI", Service.ServiceUrl.AbsoluteUri},
                {"Service Port", Service.Port.ToString()},
                {"Process ID", Service.ProcessId.ToString()}
            };

            WriteLogToOutput("WebDriver Service Settings", webDriver);

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

        private static void WriteLogToOutput(string logSectionName, Dictionary<string, string> logInfo)
        {
            Console.WriteLine($"{logSectionName.ToUpper()}");

            foreach (var kvp in logInfo)
            {
                Console.WriteLine($"{kvp.Key,-25}\t{kvp.Value,-30}");
            }

            Console.WriteLine($"{new string('=', 80)}");
        }

        /// <summary>
        /// Saves the WebDriver log(s) as LOG file(s).
        /// </summary>
        private void LogWebDriverLogs(TestData testData)
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
                var logName = testData.TestContext.SafeTestName + "." + logType.ToUpper() +  (logType == "har" ? ".har" : ".md");

                var logPath = Path.Combine(testData.TestContext.LogDirectory, logName);
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

        /// <summary>
        /// Saves the Service log as an MD file.
        /// </summary>
        private void LogDriverServiceLog(TestData testData)
        {
            switch (BrowserSettings.Name)
            {
                case DriverType.Chrome:
                    RenameServiceLog(testData, "chromedriver");
                    break;
                case DriverType.Ie:
                    RenameServiceLog(testData, "IEDriverServer");
                    break;
                case DriverType.PhantomJs:
                    RenameServiceLog(testData, "phantomjsdriver");
                    break;
                case DriverType.Edge:
                    RenameServiceLog(testData, "Edge");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BrowserSettings.Name), (int)BrowserSettings.Name, null);
            }
        }

        /// <summary>
        /// Rename the service log to use the test name.
        /// </summary>
        /// <param name="browserName"></param>
        private void RenameServiceLog(TestData testData, string browserName)
        {
            var sourceName = browserName + ".log";
            var destName = testData.TestContext.SafeTestName + "." + browserName.ToUpper() + ".md";
            var source = Path.Combine(testData.TestContext.BinariesDirectory, sourceName);
            var dest = Path.Combine(testData.TestContext.LogDirectory, destName);

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

        /// <summary>
        /// Saves the browser screenshot as a PNG file.
        /// </summary>
        private string LogScreenshot(TestData testData)
        {
            if (Driver == null) return null;

            var ssFileName = testData.TestContext.SafeTestName + ".png";
            var ssFilePath = Path.Combine(testData.TestContext.LogDirectory, ssFileName);

            // Take a screenshot.
            var ss = ((ITakesScreenshot)Driver).GetScreenshot();

            if (string.IsNullOrEmpty(ss.ToString()))
            {
                Console.WriteLine(
                    "Screenshot Not Taken: A webpage error may be preventing the screenshot from being taken.\r\nRepro the test manually to identify the webpage error.");
                return null;
            }

            var ssFile = SaveScreenshot(ss, ssFilePath);
            WriteFilePathToTestOutput(ssFile);
            return ssFile;
        }

        /// <summary>
        /// Saves the browser Page Source as an HTML file.
        /// </summary>
        private void LogPageSource(TestData testData)
        {
            if (Driver == null) return;

            var psFileName = testData.TestContext.SafeTestName + ".html";
            var psFilePath = Path.Combine(testData.TestContext.LogDirectory, psFileName);

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

        /// <summary>
        /// Maximize or resize browser window based on App.config BrowserSettings settings.
        /// </summary>
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
        /// Writes the file path as a file URI to the console.
        /// This creates a hyperlink to the file in a text editor.
        /// </summary>
        /// <param name="file"></param>
        private static void WriteFilePathToTestOutput(string file)
        {
            if (File.Exists(file))
            {
                // How to create a file URI.
                // ReSharper disable once UnusedVariable
                var uri = new UriBuilder
                {
                    Scheme = "file",            // The Internet access 'file' protocol.
                    Host = GetFqdn(),           // A DNS-style domain name or IP address.
                    Port = -1,                  // If the portNumber is set to a value of -1, this indicates that the default port value for the scheme will be used to connect to the host.
                    Path =  file                // The path to the Internet resource.
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

        /// <summary>
        /// Gets a fully qualified DNS Host name (FQDN) for the machine.
        /// </summary>
        /// <returns>A fully qualified DNS Host name (FQDN)</returns>
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
