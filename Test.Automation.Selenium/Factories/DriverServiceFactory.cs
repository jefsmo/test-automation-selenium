using System;
using System.Diagnostics;
using System.IO;
using NetFwTypeLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using Test.Automation.Selenium.Enums;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium.Factories
{
    /// <summary>
    /// Represents a WebDriver DriverService factory.
    /// </summary>
    internal static class DriverServiceFactory
    {
        /// <summary>
        /// Returns a DriverService instance for the browser type specified.
        /// A DriverService instance is created once for all the tests run in the class.
        /// </summary>
        /// <param name="browser">the DriverType (i.e. browser) to be used for testing</param>
        /// <param name="binariesPath">the path to the binaries directory</param>
        /// <returns>DriverService instance</returns>
        internal static DriverService CreateDriverService(BrowserSettings browser, string binariesPath)
        {
            switch (browser.Name)
            {
                case DriverType.Chrome:
                    KillWebDriverProcess("chromedriver");
                    return CreateChromeDriverService(browser, binariesPath);
                case DriverType.Ie:
                    KillWebDriverProcess("IEDriverServer");
                    return CreateIeDriverService(browser, binariesPath);
                case DriverType.PhantomJs:
                    KillWebDriverProcess("phantomjs");
                    return CreatePhantomJsDriverService(browser, binariesPath);
                case DriverType.Edge:
                    KillWebDriverProcess("MicrosoftWebDriver");
                    return CreateEdgeDriverService(browser, binariesPath);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browser.Name), (int)browser.Name, "Invalid browser name.");
            }
        }

        private static DriverService CreateEdgeDriverService(BrowserSettings browser, string binariesPath)
        {
            ////var logPath = Path.Combine(binariesPath, "MicrosoftWebDriver.log");

            var edge = EdgeDriverService.CreateDefaultService(binariesPath, "MicrosoftWebDriver.exe");
            edge.HideCommandPromptWindow = browser.HideCommandPromptWindow;
            if (Debugger.IsAttached)
            {
                edge.UseVerboseLogging = true;
            }
            edge.Port = 17556;

            return edge;
        }

        private  static ChromeDriverService CreateChromeDriverService(BrowserSettings browser, string binariesPath)
        {
            var logPath = Path.Combine(binariesPath, "chromedriver.log");

            var chromedriverService = ChromeDriverService.CreateDefaultService(binariesPath, "chromedriver.exe");
            chromedriverService.HideCommandPromptWindow = browser.HideCommandPromptWindow;
            if (Debugger.IsAttached) chromedriverService.LogPath = logPath;
            chromedriverService.EnableVerboseLogging = false;     // Sets DEBUG logging level for file in .LogPath path.
            chromedriverService.Port = 9515;

            return chromedriverService;
        }

        private static InternetExplorerDriverService CreateIeDriverService(BrowserSettings browser, string binariesPath)
        {
            var webDriver = Path.Combine(binariesPath, "IEDriverServer.exe");
            var logFile = Path.Combine(binariesPath, "IEDriverServer.log");

            ConfigureFirewallPortRule("Command line server for the IE driver", webDriver);

            var ieDriverService = InternetExplorerDriverService.CreateDefaultService(binariesPath, "IEDriverServer.exe");
            ieDriverService.HideCommandPromptWindow = browser.HideCommandPromptWindow;
            if (Debugger.IsAttached) ieDriverService.LogFile = logFile;
            ieDriverService.LoggingLevel = InternetExplorerDriverLogLevel.Info;    // Sets Logging level for file in .LogFile path.
            ieDriverService.Port = 5555;

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
        /// <param name="browser"></param>
        /// <param name="binariesPath">the path to the binaries directory</param>
        private static PhantomJSDriverService CreatePhantomJsDriverService(BrowserSettings browser, string binariesPath)
        {
            var webDriver = Path.Combine(binariesPath, "phantomjs.exe");
            var logFile = Path.Combine(binariesPath, "phantomjsdriver.log");

            ConfigureFirewallPortRule("Phantom JS is a headless WebKit with JavaScript API", webDriver);

            var phantomJs = PhantomJSDriverService.CreateDefaultService(binariesPath, "phantomjs.exe");
            phantomJs.HideCommandPromptWindow = browser.HideCommandPromptWindow;
            if (Debugger.IsAttached) phantomJs.LogFile = logFile;
            phantomJs.Port = 8910;

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
            if (string.IsNullOrEmpty(processName)) return;

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
