using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NetFwTypeLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents a factory for methods to create a DriverService and RemoteWebDriver for testing with a desired browser.
    /// </summary>
    /// <remarks>
    /// Log type	    Meaning
    /// -----------     -------------------------------------------
    /// BROWSER         Javascript console logs from the browser
    /// CLIENT          Logs from the client side implementation of the WebDriver protocol(e.g.the Java bindings)
    /// DRIVER          Logs from the internals of the driver(e.g.FirefoxDriver internals)
    /// PERFORMANCE     Logs relating to the performance characteristics of the page under test(e.g.resource load timings)
    /// SERVER          Logs from within the selenium server.
    /// </remarks>
    internal static class WebDriverFactory
    {
        /// <summary>
        /// Returns a DriverService instance for the WebDriver type specified.
        /// A single DriverService instance is created for all the tests run in the test class.
        /// </summary>
        /// <param name="browserSettings">The WebDriver type browser settings (i.e. browser) to be used for testing.</param>
        /// <param name="binariesPath">The path to the binaries directory.</param>
        /// <returns>DriverService instance</returns>
        internal static DriverService CreateDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            switch (browserSettings.Name)
            {
                case DriverType.Chrome:
                    KillWebDriverProcess("chromedriver");
                    return CreateChromeDriverService(browserSettings, binariesPath);
                case DriverType.Ie:
                    KillWebDriverProcess("IEDriverServer");
                    return CreateIeDriverService(browserSettings, binariesPath);
                case DriverType.PhantomJs:
                    KillWebDriverProcess("phantomjs");
                    return CreatePhantomJsDriverService(browserSettings, binariesPath);
                case DriverType.Edge:
                    KillWebDriverProcess("MicrosoftWebDriver");
                    return CreateEdgeDriverService(browserSettings, binariesPath);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browserSettings.Name), (int)browserSettings.Name, "Invalid browser name.");
            }
        }

        /// <summary>
        /// Returns a RemoteWebDriver instance for the WebDriver type specified.
        /// A new RemoteWebDriver instance is created for each test.
        /// </summary>
        /// <param name="browserSettings">The WebDriver type browser settings (i.e. browser) to be used for testing.</param>
        /// <param name="driverservice">The browser specific DriverService to use when creating the WebDriver.</param>
        /// <returns>WebDriver instance</returns>
        internal static RemoteWebDriver CreateWebDriver(BrowserSettings browserSettings, DriverService driverservice)
        {
            switch (browserSettings.Name)
            {
                case DriverType.Chrome:
                    return CreateChromeDriver(driverservice, browserSettings);
                case DriverType.Ie:
                    return CreateIeDriver(driverservice);
                case DriverType.PhantomJs:
                    return CreatePhantomDriver(driverservice);
                case DriverType.Edge:
                    return CreateMicrosoftWebDriver(driverservice);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browserSettings.Name), (int)browserSettings.Name, "Invalid browser name.");
            }
        }

        private static RemoteWebDriver CreateMicrosoftWebDriver(DriverService service)
        {
            var edgeOptions = new EdgeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal
            };

            edgeOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
            edgeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            try
            {
                return new RemoteWebDriver(service.ServiceUrl, edgeOptions);
            }
            catch (InvalidOperationException ioEx)
            {
                Console.WriteLine("The browser is not installed on the machine. Edit the app.config file to use an installed browser.");
                Console.WriteLine(ioEx);
                throw;
            }
        }

        private static RemoteWebDriver CreateChromeDriver(DriverService service, BrowserSettings browser)
        {
            // CAUTION: Using the "no-sandbox" option leaves 2 chrome processes open after calling Driver.Quit();
            //          It appears that "no-sandbox" is no longer required for running in Visual Studio.
            var args = new List<string>
            {
                "disable-plugins", "disable-plugins-discovery", "disable-extensions"
            };

            if (browser.IsHeadless)
            {
                args.AddRange(new List<string>{
                    "headless",
                    "disable-gpu"
                });
            }

            if (browser.IsMaximized)
            {
                args.Add("start-maximized");
            }
            else
            {
                args.Add($"window-position={browser.Position.X},{browser.Position.Y}");
                args.Add($"window-size={browser.Size.Width},{browser.Size.Height}");
            }

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(args);

            if (!string.IsNullOrEmpty(browser.DownloadDefaultDir))
            {
                chromeOptions.AddUserProfilePreference("download.default_directory", $"{browser.DownloadDefaultDir}");
            }

            // Chrome supports DRIVER and BROWSER.
            chromeOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            try
            {
                return new RemoteWebDriver(service.ServiceUrl, chromeOptions);
            }
            catch (InvalidOperationException ioEx)
            {
                Console.WriteLine("The browser is not installed on the machine. Edit the app.config file to use an installed browser.");
                Console.WriteLine(ioEx);
                throw;
            }
        }

        private static RemoteWebDriver CreateIeDriver(DriverService service)
        {
            var ieOptions = new InternetExplorerOptions
            {
                EnsureCleanSession = true,
                IgnoreZoomLevel = true,
            };

            ieOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
            ieOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            try
            {
                return new RemoteWebDriver(service.ServiceUrl, ieOptions);
            }
            catch (InvalidOperationException ioEx)
            {
                Console.WriteLine("The browser is not installed on the machine. Edit the app.config file to use an installed browser.");
                Console.WriteLine(ioEx);
                throw;
            }
        }

        /// <summary>
        /// Creates an istance of GhostDriver (embedded in PhantomJS.) This is the embedded GhostDriver. 
        /// </summary>
        /// <remarks> 
        /// https://raw.githubusercontent.com/wiki/SeleniumHQ/selenium/DesiredCapabilities.md
        /// ==================================================================
        ///     --port=PORT_GHOSTDRIVER_SHOULD_LISTEN_ON
        ///     --logFile=PATH_TO_LOGFILE
        ///     --logLevel=(INFO|DEBUG|WARN|ERROR)
        ///     --logColor=(false|true)
        /// phantomJsOptions.AddAdditionalCapability("phantomjs.ghostdriver.cli.args", new[] { "--logLevel=INFO", $"--logFile={TestContext.CurrentContext.TestDirectory}\\ghostdriver.log" });
        /// ===================================================================
        ///     --webdriver-logfile=[val]            File where to write the WebDriver's Log (default 'none') (NOTE: needs '--webdriver')
        ///     --webdriver-loglevel=[val]           WebDriver Logging Level: (supported: 'ERROR', 'WARN', 'INFO', 'DEBUG') (default 'INFO') (NOTE: needs '--webdriver')
        /// phantomJsOptions.AddAdditionalCapability("phantom.cli.args", new[] { "--wd", "--webdriver-loglevel=INFO", $"--webdriver-logfile={TestContext.CurrentContext.TestDirectory}\\phantomjs.log"});
        /// </remarks>
        /// <param name="service"></param>
        /// <returns></returns>
        private static RemoteWebDriver CreatePhantomDriver(DriverService service)
        {
            var phantomJsOptions = new PhantomJSOptions();
            if (Debugger.IsAttached)
            {
                // PhantomJS supports BROWSER and NETWORK (HAR).
                phantomJsOptions.SetLoggingPreference(LogType.Driver, LogLevel.Debug);
                phantomJsOptions.SetLoggingPreference(LogType.Browser, LogLevel.Debug);
            }

            try
            {
                return new RemoteWebDriver(service.ServiceUrl, phantomJsOptions.ToCapabilities());
            }
            catch (InvalidOperationException ioEx)
            {
                Console.WriteLine("The browser is not installed on the machine. Edit the app.config file to use an installed browser.");
                Console.WriteLine(ioEx);
                throw;
            }
        }

        private static DriverService CreateEdgeDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            var edgeDriverService = EdgeDriverService.CreateDefaultService(binariesPath, "MicrosoftWebDriver.exe");
            edgeDriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            edgeDriverService.Port = 17556;

            if (Debugger.IsAttached)
            {
                edgeDriverService.UseVerboseLogging = true;
            }

            return edgeDriverService;
        }

        private static DriverService CreateChromeDriverService(BrowserSettings browserSettings, string binariesPath)
        {

            var chromedriverService = ChromeDriverService.CreateDefaultService(binariesPath, "chromedriver.exe");
            chromedriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            chromedriverService.Port = 9515;

            if (Debugger.IsAttached)
            {
                var logPath = Path.Combine(binariesPath, "chromedriver.log");
                chromedriverService.LogPath = logPath;
                chromedriverService.EnableVerboseLogging = true;     // Sets DEBUG logging level for file in .LogPath path.
            }

            return chromedriverService;
        }

        private static DriverService CreateIeDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            var webDriver = Path.Combine(binariesPath, "IEDriverServer.exe");
            ConfigureFirewallPortRule("Command line server for the IE driver", webDriver);
            var ieDriverService = InternetExplorerDriverService.CreateDefaultService(binariesPath, "IEDriverServer.exe");
            ieDriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            ieDriverService.Port = 5555;

            if (Debugger.IsAttached)
            {
                var logFile = Path.Combine(binariesPath, "IEDriverServer.log");
                ieDriverService.LogFile = logFile;
                ieDriverService.LoggingLevel = InternetExplorerDriverLogLevel.Debug;    // Sets Logging level for file in .LogFile path.
            }

            return ieDriverService;
        }

        /// <summary>
        /// Creates an instance of PhantomJSDriverService.
        /// Options set in PhantomJSDriverService affect phantomjs.exe.
        ///</summary>
        /// <remarks>
        /// Use HAR viewer to view .HAR (HTTP archive) files. HAR viewer for Chrome extension does not work??
        /// When LogFile is null or empty string, the output goes to the console instead of a file.
        /// NOTE:  var logFile = "phantomjsdriver.log"; // Goes to C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE
        /// </remarks>
        /// <param name="browserSettings"></param>
        /// <param name="binariesPath">the path to the binaries directory</param>
        private static PhantomJSDriverService CreatePhantomJsDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            var webDriver = Path.Combine(binariesPath, "phantomjs.exe");
            ConfigureFirewallPortRule("Phantom JS is a headless WebKit with JavaScript API", webDriver);
            var phantomJs = PhantomJSDriverService.CreateDefaultService(binariesPath, "phantomjs.exe");
            phantomJs.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            phantomJs.Port = 8910;

            if (Debugger.IsAttached)
            {
                var logFile = Path.Combine(binariesPath, "phantomjsdriver.log");
                phantomJs.LogFile = logFile;
            }

            return phantomJs;
        }

        /// <summary>
        /// Adds TCP and UDP firewall rules for WebDrivers that require a firewall rule.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Add COM DLL NetFwTypeLib as a reference.
        /// Visual Studio must be run with elevated or Admin privileges to add a firewall rule.
        /// Alternatively, click 'Allow' when the firewall diaglog box appears.
        /// </remarks>
        /// <remarks>
        /// Known WebDrivers that require a firewall rule:
        ///     IEDriverServer.exe = "Command line server for the IE driver"
        ///     phantomjs.exe = "Phantom JS is a headless WebKit with JavaScript API"
        /// </remarks>
        /// <param name="firewallRuleName">the rule name</param>
        /// <param name="webDriver">the full path to the webdriver exe</param>
        private static void ConfigureFirewallPortRule(string firewallRuleName, string webDriver)
        {
            // Create the FwPolicy 2 object
            var fwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            var fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(fwPolicy2Type);

            // check if rule exists.
            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                if (rule.Name == firewallRuleName)
                {
                    fwPolicy2.Rules.Remove(rule.Name);
                }
            }

            var newRule = CreateProtocolRule(firewallRuleName, webDriver);

            try
            {
                // Add the new rule.
                fwPolicy2.Rules.Add(newRule);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine("[DriverServiceFactory]: An error occurred attempting to add Firewall rule.");
                Console.WriteLine(uaEx);
                Console.WriteLine("**********\r\n** To add Firewall rule, run Visual Studio using elevated privileges.\r\n** Alternatively, use the manual workaround -- click 'Allow' when the Firewall dialog box appears.\r\n**********");
            }
        }

        private static INetFwRule CreateProtocolRule(string firewallRuleName, string webDriver)
        {
            // Create an INetFwRule rule.
            var newTcpRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

            // TCP-In Rule settings:
            newTcpRule.Grouping = "-automation-";
            newTcpRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            newTcpRule.Name = firewallRuleName;
            newTcpRule.Description = firewallRuleName + " Added by test automation. OK to delete.";
            newTcpRule.Enabled = true;
            newTcpRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            newTcpRule.ApplicationName = webDriver;
            newTcpRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
            newTcpRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;

            return newTcpRule;
        }

        /// <summary>
        /// Checks if WebDriver process is running and immediately stops the process.
        /// Remove the '.exe' extension from the process name.
        /// [ chromedriver | IEDriverServer | phantomjs ]
        /// </summary>
        private static void KillWebDriverProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return;

            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    Console.WriteLine($"Kill process [{process.ProcessName}]");
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DriverServiceFactory]: Failed To Kill Process: [{process.ProcessName}]");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
