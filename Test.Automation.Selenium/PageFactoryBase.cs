using System;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents an abstract page factory base class.
    /// </summary>
    public abstract class PageFactoryBase
    {
        private static string TestEnvironment
            => ConfigurationManager.AppSettings["testRunSetting"];
        private static double DefaultWaitTimeout
            => ((BrowserSettings)ConfigurationManager.GetSection("browserSettings/" + TestEnvironment)).DefaultWaitTimeout;

        /// <summary>
        /// The base URI of the Application Under Test.
        /// </summary>
        protected static Uri BaseUri
            => ((EnvironmentSettings)ConfigurationManager.GetSection("environmentSettings/" + TestEnvironment)).BaseUri;

        /// <summary>
        /// The default WebDriver Wait.
        /// </summary>
        protected WebDriverWait Wait { get; set; }

        /// <summary>
        /// The current WebDriver instance.
        /// </summary>
        protected IWebDriver Driver { get; set; }

        /// <summary>
        /// The Page Title.
        /// </summary>
        protected abstract string Title { get; }

        /// <summary>
        /// The Page URI.
        /// </summary>
        protected abstract Uri PageUri { get; }

        /// <summary>
        /// Creates an instance of the Page Factory base class with re-trying locators.
        /// </summary>
        /// <param name="driver">The current WebDriver instance.</param>
        protected PageFactoryBase(IWebDriver driver)
        {
            Driver = driver;
            var timeout = TimeSpan.FromSeconds(DefaultWaitTimeout);
            Wait = new WebDriverWait(driver, timeout);

            var locator = new RetryingElementLocator(driver, timeout);
            var decorator = new DefaultPageObjectMemberDecorator();
            PageFactory.InitElements(this, locator, decorator);
        }
    }
}
