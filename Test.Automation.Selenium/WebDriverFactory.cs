using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NetFwTypeLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents a factory for methods to create a DriverService and RemoteWebDriver for testing with a desired browser.
    /// </summary>
    /// <remarks>
    /// WebDriver instance logging LogLevels
    /// Log Levels
    /// ------------
    /// OFF: Turns off logging
    /// SEVERE: Messages about things that went wrong. For instance, an unknown command.
    /// WARNING:  Messages about things that may be wrong but was handled. For instance, a handled exception.
    /// INFO: Messages of an informative nature. For instance, information about received commands.
    /// DEBUG: Messages for debugging. For instance, information about the state of the driver.
    /// ALL: All log messages. A way to collect all information regardless of which log levels that are supported.
    /// </remarks>
    /// <remarks>
    /// WebDriver Instance logging LogType
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
                case DriverType.MicrosoftEdge:
                    KillWebDriverProcess("MicrosoftWebDriver");
                    return CreateEdgeDriverService(browserSettings, binariesPath);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browserSettings.Name), browserSettings.Name, "Invalid argument for browser name - does not match DriverType.");
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
                case DriverType.MicrosoftEdge:
                    return CreateMicrosoftWebDriver(driverservice, browserSettings);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browserSettings.Name), browserSettings.Name, "Invalid argument for browser name - does not match DriverType.");
            }
        }

        #region PRIVATE METHODS

        private static RemoteWebDriver CreateMicrosoftWebDriver(DriverService service, BrowserSettings browserSettings)
        {
            /**************************************************************
             * MicrosoftEdge does not support instance logging.
             * ***********************************************************/

            var edgeOptions = new EdgeOptions
            {
                PageLoadStrategy = browserSettings.PageLoadStrategy,
                StartPage = browserSettings.InitialBrowserUrl
            };

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
            /******************************************************************
             *  CAUTION: Using the "no-sandbox" option leaves 2 chrome processes open after calling Driver.Quit();
             *  It appears that "no-sandbox" is no longer required for running in Visual Studio IDE.
             * ***************************************************************/

            var args = new List<string>
            {
                "disable-plugins",
                "disable-plugins-discovery",
                "disable-extensions"
            };

            // Headless Chrome setting.
            if (browserSettings.IsHeadless)
            {
                var resolution = Screen.PrimaryScreen.Bounds;

                // Force Chrome to start maximized when headless.
                args.AddRange(new List<string>
                {
                    "headless",
                    // BUG: Chrome requires disable-gpu in headlesss mode.
                    "disable-gpu",
                    "start-maximized",
                    // BUG: Chrome requires window-size in headless mode.
                    $"window-size={resolution.Width + ", " + resolution.Height}"
                });
            }
            else if (browserSettings.IsMaximized)
            {
                args.Add("start-maximized");
            }
            else
            {
                args.Add($"window-position={browserSettings.Position.X},{browserSettings.Position.Y}");
                args.Add($"window-size={browserSettings.Size.Width},{browserSettings.Size.Height}");
            }

            var chromeOptions = new ChromeOptions
            {
                PageLoadStrategy = browserSettings.PageLoadStrategy
            };

            chromeOptions.AddArguments(args);

            // TODO:  Chrome start page setting does not seem to work...
            //if (! string.IsNullOrEmpty(browserSettings.InitialBrowserUrl))
            //{
            // chrome.exe https://www.bing.com  <=== this only works on the command line!
            // Default behavior: The 'data:,' URL is the default address while launching chrome after which it navigates to the given url.
            // --homepage: Specifies which page will be displayed in newly-opened tabs; must manually open tab...
            // --app: Specifies that the associated value should be launched in "application" mode.
            // --new-window: Launches URL in new browser window; opens 2nd tab with new URL...
            // 
            //chromeOptions.AddArgument($"{browserSettings.InitialBrowserUrl}"); gets treated as an argument, not a URL: --https:www.bing.com instead of https://www.bing.com
            //}

            // Chrome DownloadDefaultDir setting.
            if (!string.IsNullOrEmpty(browserSettings.DownloadDefaultDir))
            {
                chromeOptions.AddUserProfilePreference("download.default_directory", $"{browserSettings.DownloadDefaultDir}");
            }

            // Chrome supports DRIVER and BROWSER instance logs.
            chromeOptions.SetLoggingPreference(LogType.Browser, browserSettings.LogLevel);
            chromeOptions.SetLoggingPreference(LogType.Driver, browserSettings.LogLevel);

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
            /**************************************************************
             * The IE driver does not support getting any instance logs.
             * Those commands have not been implemented.
             * 
             * IE WebDriver may throw an exception if 'Enable Protected Mode' internet options security setting is not the same for all zones.
             *
             * By setting the OpenQA.Selenium.IE.InternetExplorerOptions.IntroduceInstabilityByIgnoringProtectedModeSettings to true
             * and InitialBrowserUrl property to a correct URL, you can launch IE in the Internet Protected Mode zone.
             * This can be helpful to avoid the flakiness introduced by ignoring the Protected Mode settings.
             * Nevertheless, setting Protected Mode zone settings to the same value in the IE configuration is the preferred method.
             * ***********************************************************/

            var ieOptions = new InternetExplorerOptions
            {
                EnsureCleanSession = browserSettings.EnsureCleanSession,  // Clears the IE cache.
                IgnoreZoomLevel = false,    // The IE zoom level should be set to 100% or the driver may not interact with page controls correctly.
                PageLoadStrategy = browserSettings.PageLoadStrategy,
                InitialBrowserUrl = browserSettings.InitialBrowserUrl,
                IntroduceInstabilityByIgnoringProtectedModeSettings = browserSettings.IgnoreProtectedModeSettings
            };

            // 'timeouts' object: Time (in ms) that time-limited commands are permitted to run.
            var timeouts = new
            {
                @implicit = 0,      // No time limit for the implicit wait timeout.
                pageLoad = 300000,  // 30 s. for the page load timeout.
                script = 0          // No time limit for script timeouts.
            };

            /******************************************************************
             * NOTE: A warning is logged to the IEDriverServer log b/c by default the "timeouts" capability is set to null.
             * This code replaces null with a valid 'timeouts' object with default timeout values.
             * ***************************************************************/
            ieOptions.AddAdditionalCapability(CapabilityType.Timeouts, timeouts, true);

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
            /******************************************************************
             * For Windows 10 after OS version 17134: install MicrosoftWebDriver as a Feature on Demand, insead of a download:
             * https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/
             * DISM.exe /Online /Add-Capability /CapabilityName:Microsoft.WebDriver~~~~0.0.1.0
             * ***************************************************************/

            var edgeDriverService = EdgeDriverService.CreateDefaultService(binariesPath, "MicrosoftWebDriver.exe");
            edgeDriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            edgeDriverService.Port = 17556;
            edgeDriverService.SuppressInitialDiagnosticInformation = false;

            if (Debugger.IsAttached)
            {
                /**************************************************************
                 * There is no setting for LogFile/LogPath! We do not expect to see a service log.
                 * ***********************************************************/
                //var logPath = Path.Combine(binariesPath, DriverType.MicrosoftEdge.ToDescription() + ".log");
                edgeDriverService.UseVerboseLogging = browserSettings.EnableVerboseLogging;
            }

            return edgeDriverService;
        }

        private static DriverService CreateChromeDriverService(BrowserSettings browserSettings, string binariesPath)
        {
            var chromedriverService = ChromeDriverService.CreateDefaultService(binariesPath, "chromedriver.exe");
            chromedriverService.HideCommandPromptWindow = browserSettings.HideCommandPromptWindow;
            chromedriverService.Port = 9515;
            chromedriverService.SuppressInitialDiagnosticInformation = false;

            if (Debugger.IsAttached)
            {
                chromedriverService.LogPath = Path.Combine(binariesPath, DriverType.Chrome.ToDescription() + ".log");

                // Sets the logging level = DEBUG when true for file in .LogPath path; else INFO.
                // Equivalent to:  chromedriver.exe --verbose --log-path=chromedriver.log
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
            ieDriverService.SuppressInitialDiagnosticInformation = false;
            ieDriverService.Port = 5555;

            if (Debugger.IsAttached)
            {
                ieDriverService.LogFile = Path.Combine(binariesPath, DriverType.IE.ToDescription() + ".log");

                // IE is different. It can set multiple logging levels.
                // Sets the logging level = DEBUG when true for file in .LogPath path; else INFO.
                ieDriverService.LoggingLevel = browserSettings.EnableVerboseLogging
                    ? InternetExplorerDriverLogLevel.Debug
                    : InternetExplorerDriverLogLevel.Info;
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
        /// 1. IEDriverServer.exe = "Command line server for the IE driver"
        /// 2. phantomjs.exe = "Phantom JS is a headless WebKit with JavaScript API"
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
