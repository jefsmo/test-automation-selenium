using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using OpenQA.Selenium;

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
        /// Default = 1600, 900.
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
        /// Gets a default Chrome browser download directory. (Chrome only.)
        /// </summary>
        [ConfigurationProperty("DownloadDefaultDir", IsRequired = false)]
        public string DownloadDefaultDir => (string)this["DownloadDefaultDir"];

        /// <summary>
        /// Gets the setting for running Chrome in a headless environment. (Chrome only.)
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("IsHeadless", IsRequired = false, DefaultValue = false)]
        public bool IsHeadless => (bool)this["IsHeadless"];

        /// <summary>
        /// Gets the EnableVerboseLogging setting for DriverService logging.
        /// Default = false.
        /// Verbose is DEBUG level, otherwise it is INFO level.
        /// </summary>
        [ConfigurationProperty("EnableVerboseLogging", IsRequired = false, DefaultValue = false)]
        public bool EnableVerboseLogging => (bool)this["EnableVerboseLogging"];

        /// <summary>
        /// Gets the LogLevel setting to determine levels of logging available to WebDriver instances (usually BROWSER and DRIVER.)
        /// Default = LogLevel.Warning.
        /// [ All | Debug | Info | *Warning | Severe | Off ]
        /// </summary>
        [ConfigurationProperty("LogLevel", IsRequired = false, DefaultValue = LogLevel.Warning)]
        public LogLevel LogLevel => (LogLevel)this["LogLevel"];

        /// <summary>
        /// Gets the IntroduceInstabilityByIgnoringProtectedModeSettings setting. (IE only.)
        /// Default = false.
        /// Use to fix exception when IE 'Enable Protected Mode' is not checked/un-checked for all zones.
        /// </summary>
        [ConfigurationProperty("IntroduceInstabilityByIgnoringProtectedModeSettings", IsRequired = false, DefaultValue = false)]
        public bool IntroduceInstabilityByIgnoringProtectedModeSettings => (bool)this["IntroduceInstabilityByIgnoringProtectedModeSettings"];
    }
}
