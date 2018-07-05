using System.ComponentModel;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Maps browsers to  browser specific WebDrivers.
    /// </summary>
    public enum DriverType
    {
        /// <summary>
        /// Chrome = chromedriver.exe
        /// </summary>
        [Description("chromedriver.exe")]
        Chrome,

        /// <summary>
        /// Internet Explorer = IEDriverServer.exe
        /// </summary>
        [Description("IEDriverServer.exe")]
        IE,

        /// <summary>
        /// MicrosoftEdge = MicrosoftWebDriver.exe
        /// </summary>
        [Description("MicrosoftWebDriver.exe")]
        MicrosoftEdge
    }
}
