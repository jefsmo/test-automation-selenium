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
                case DriverType.IE:
                    KillWebDriverProcess("IEDriverServer");
                    return CreateIeDriverService(browserSettings, binariesPath);
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
                case DriverType.IE:
                    return CreateIeDriver(driverservice, browserSettings);
                case DriverType.Edge:
                    return CreateMicrosoftWebDriver(driverservice, browserSettings);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browserSettings.Name), (int)browserSettings.Name, "Invalid browser name.");
            }
        }

        #region PRIVATE METHODS
        private static RemoteWebDriver CreateMicrosoftWebDriver(DriverService service, BrowserSettings browserSettings)
        {
            var edgeOptions = new EdgeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal
            };

            edgeOptions.SetLoggingPreference(LogType.Driver, browserSettings.LogLevel);
            edgeOptions.SetLoggingPreference(LogType.Browser, browserSettings.LogLevel);

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

        private static RemoteWebDriver CreateChromeDriver(DriverService service, BrowserSettings browserSettings)
        {
            // CAUTION: Using the "no-sandbox" option leaves 2 chrome processes open after calling Driver.Quit();
            //          It appears that "no-sandbox" is no longer required for running in Visual Studio.
            var args = new List<string>
            {
                "disable-plugins", "disable-plugins-discovery", "disable-extensions"
            };

            if (browserSettings.IsHeadless)
            {
                args.AddRange(new List<string>{
                    "headless",
                    "disable-gpu"
                });
            }

            if (browserSettings.IsMaximized)
            {
                args.Add("start-maximized");
            }
            else
            {
                args.Add($"window-position={browserSettings.Position.X},{browserSettings.Position.Y}");
                args.Add($"window-size={browserSettings.Size.Width},{browserSettings.Size.Height}");
            }

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(args);

            if (!string.IsNullOrEmpty(browserSettings.DownloadDefaultDir))
            {
                chromeOptions.AddUserProfilePreference("download.default_directory", $"{browserSettings.DownloadDefaultDir}");
            }

            // Chrome supports DRIVER and BROWSER.
            chromeOptions.SetLoggingPreference(LogType.Driver, browserSettings.LogLevel);
            chromeOptions.SetLoggingPreference(LogType.Browser, browserSettings.LogLevel);

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

        private static RemoteWebDriver CreateIeDriver(DriverService service, BrowserSettings browserSettings)
        {
            var ieOptions = new InternetExplorerOptions
            {
                EnsureCleanSession = true,
                IgnoreZoomLevel = true,
                PageLoadStrategy = PageLoadStrategy.Normal,
                // IE requires 'Enable Protected Mode' security setting to be the same for all zones.
                // This setting is a workaround for that requirement.
                IntroduceInstabilityByIgnoringProtectedModeSettings = browserSettings.IntroduceInstabilityByIgnoringProtectedModeSettings
            };

            // IEDriverServer log JSON:
            //"timeouts" : 
			//{
            //    "implicit" : 0,
			//	  "pageLoad" : 300000,
			//	  "script" : 0
            //}
            // Time (in ms) that time-limited commands are permitted to run.
            // Default = 0 ms means no  time limit.
            // Valid values are: "script" for script timeouts, "implicit" for modifying the implicit wait timeout and "page load" for setting a page load timeout.
            ieOptions.AddAdditionalCapability(CapabilityType.Timeouts, new { @implicit = 0, pageLoad = 300000, script = 0 }, true);

            //  The IE driver does not support getting logs of any kind.
            //  Those commands have not been implemented.
            //  They will be implemented only when the server-side API is properly specified.
            ieOptions.SetLoggingPreference(LogType.Driver, browserSettings.LogLevel);
            ieOptions.SetLoggingPreference(LogType.Browser, browserSettings.LogLevel);

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

        private static DriverService CreateEdgeDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            var edgeDriverService = EdgeDriverService.CreateDefaultService(binariesPath, "MicrosoftWebDriver.exe");
            edgeDriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            edgeDriverService.Port = 17556;

            if (Debugger.IsAttached)
            {
                // Sets the logging level = DEBUG when true for file in .LogPath path; else WARN.
                edgeDriverService.UseVerboseLogging = browserSettings.EnableVerboseLogging;
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

                // Sets the logging level = DEBUG when true for file in .LogPath path; else WARN.
                chromedriverService.EnableVerboseLogging = browserSettings.EnableVerboseLogging;     
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

                // IE is different. It can set multiple logging levels.
                // Sets the logging level = DEBUG when true for file in .LogPath path; else WARN.
                ieDriverService.LoggingLevel = browserSettings.EnableVerboseLogging
                    ? InternetExplorerDriverLogLevel.Debug
                    : InternetExplorerDriverLogLevel.Warn;
            }

            return ieDriverService;
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
        /// [ chromedriver | IEDriverServer ]
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
        #endregion
    }
}
