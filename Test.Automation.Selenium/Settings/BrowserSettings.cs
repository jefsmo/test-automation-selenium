using System.ComponentModel;
using System.Configuration;
using System.Drawing;
// ReSharper disable ClassNeverInstantiated.Global

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents the App.config BrowserSettings data.
    /// </summary>
    public sealed class BrowserSettings : ConfigurationSection
    {
        /// <summary>
        /// Gets the WebDriver browser name.
        /// Default = Chrome.
        /// </summary>
        [ConfigurationProperty("Name", IsRequired = false, DefaultValue = "Chrome")]
        [TypeConverter(typeof(CustomDriverTypeConverter))]
        public DriverType Name => (DriverType)this["Name"];

        /// <summary>
        /// Gets the WebDriver browser starting window position.
        /// Default = 10, 10.
        /// </summary>
        [ConfigurationProperty("Position", IsRequired = false, DefaultValue = "10, 10")]
        [TypeConverter(typeof(CustomPointConverter))]
        public Point Position => (Point)this["Position"];

        /// <summary>
        /// Gets the WebDriver browser starting window size. 
        /// Default = 1600 x 900.
        /// </summary>
        [ConfigurationProperty("Size", IsRequired = false, DefaultValue = "1600, 900")]
        [TypeConverter(typeof(CustomSizeConverter))]
        public Size Size => (Size)this["Size"];

        /// <summary>
        /// Gets browser window IsMaximized status.
        /// This setting overrides WindowPosition and WindowSize settings.
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("IsMaximized", IsRequired = false, DefaultValue = false)]
        public bool IsMaximized => (bool)this["IsMaximized"];

        /// <summary>
        /// Gets WebDriver service HideCommandPromptWindow status.
        /// Default = true.
        /// </summary>
        [ConfigurationProperty("HideCommandPromptWindow", IsRequired = false, DefaultValue = true)]
        public bool HideCommandPromptWindow => (bool)this["HideCommandPromptWindow"];

        /// <summary>
        /// Gets a custom WebDriver Wait timeout value.
        /// Default = 3D (double).
        /// </summary>
        [ConfigurationProperty("DefaultWaitTimeout", IsRequired = false, DefaultValue = 3D)]
        [TypeConverter(typeof(DoubleConverter))]
        public double DefaultWaitTimeout => (double) this["DefaultWaitTimeout"];

        /// <summary>
        /// Sets a default Chrome browser download directory.
        /// (Chrome only)
        /// </summary>
        [ConfigurationProperty("DownloadDefaultDir", IsRequired = false)]
        public string DownloadDefaultDir => (string) this["DownloadDefaultDir"];

        /// <summary>
        /// Gets the setting for running Chrome in a headless environment.
        /// Default = false.
        /// (Chrome only)
        /// </summary>
        [ConfigurationProperty("IsHeadless", IsRequired = false, DefaultValue = false)]
        public bool IsHeadless => (bool)this["IsHeadless"];
    }
}
