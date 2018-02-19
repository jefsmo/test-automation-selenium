using System.ComponentModel;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Maps browsers to  browser specific WebDrivers.
    /// </summary>
    public enum DriverType
    {
        /// <summary>
        /// ChromeDriver.
        /// </summary>
        [Description("ChromeDriver")]
        Chrome,

        /// <summary>
        /// IEDriverServer.
        /// </summary>
        [Description("IEDriverServer")]
        Ie,

        /// <summary>
        /// PhantomJS.
        /// </summary>
        [Description("PhantomJS")]
        PhantomJs,

        /// <summary>
        /// MicrosoftWebDriver.
        /// </summary>
        [Description("MicrosoftWebDriver")]
        Edge
    }
}
