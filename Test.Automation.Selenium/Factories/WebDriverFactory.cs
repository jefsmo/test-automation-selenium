using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using Test.Automation.Selenium.Enums;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium.Factories
{
    /// <summary>
    /// Represents a WebDriver factory.
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
        /// Returns an IWebDriver instance for the browser type specified.
        /// </summary>
        /// <param name="browser">the DriverType (browser) to be used for testing</param>
        /// <param name="service">the browser specific DriverService to use when creating the WebDriver.</param>
        /// <returns>WebDriver instance</returns>
        internal static IWebDriver CreateWebDriver(BrowserSettings browser, DriverService service)
        {
            switch (browser.Name)
            {
                case DriverType.Chrome:
                    return CreateChromeDriver(service, browser);
                case DriverType.Ie:
                    return CreateIeDriver(service);
                case DriverType.PhantomJs:
                    return CreatePhantomDriver(service);
                case DriverType.Edge:
                    return CreateMicrosoftWebDriver(service);
                default:
                    Console.WriteLine("Support for this browser's WebDriver has not been added to the base class. Update the base class to use this WebDriver.");
                    throw new ArgumentOutOfRangeException(nameof(browser.Name), (int)browser.Name, "Invalid browser name.");
            }
        }

        private static IWebDriver CreateMicrosoftWebDriver(DriverService service)
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

        private static IWebDriver CreateChromeDriver(DriverService service, BrowserSettings browser)
        {
            var args = new List<string>
            {
                "no-sandbox", "disable-plugins", "disable-plugins-discovery", "disable-extensions"
            };

            if(browser.IsHeadless)
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

        private static IWebDriver CreateIeDriver(DriverService service)
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
        private static IWebDriver CreatePhantomDriver(DriverService service)
        {
            var phantomJsOptions = new PhantomJSOptions();
            // PhantomJS supports BROWSER and NETWORK (HAR).
            phantomJsOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
            phantomJsOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

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
    }
}
