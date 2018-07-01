using System.ComponentModel;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Maps browsers to  browser specific WebDrivers.
    /// </summary>
    public enum DriverType
    {
        /// <summary>
        /// Chrome.
        /// </summary>
        [Description("Chrome")]
        Chrome,

        /// <summary>
        /// Internet Explorer.
        /// </summary>
        [Description("Internet Explorer")]
        IE,

        /// <summary>
        /// MicrosoftEdge.
        /// </summary>
        [Description("MicrosoftEdge")]
        MicrosoftEdge
    }
}
