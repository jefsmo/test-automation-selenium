using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using OpenQA.Selenium;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents App.config BrowserSettings values for WebDriver browsers.
    /// </summary>
    public sealed class BrowserSettings : ConfigurationSection
    {
        /// <summary>
        /// Gets the WebDriver browser name.
        /// Default = DriverType.Chrome.
        /// </summary>
        [ConfigurationProperty("Name", IsRequired = false, DefaultValue = DriverType.Chrome)]
        [TypeConverter(typeof(CustomDriverTypeConverter))]
        public DriverType Name => (DriverType)this["Name"];

        #region DRIVERSERVICE SETTINGS

        /// <summary>
        /// Gets the setting for whether to hide the DriverService command prompt window.
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("HideCommandPromptWindow", IsRequired = false, DefaultValue = false)]
        public bool HideCommandPromptWindow => (bool)this["HideCommandPromptWindow"];

        /// <summary>
        /// Gets the setting to set the DriverService log logging level.
        /// Default = false.
        /// Verbose is DEBUG level, otherwise it is INFO level.
        /// </summary>
        [ConfigurationProperty("EnableVerboseLogging", IsRequired = false, DefaultValue = false)]
        public bool EnableVerboseLogging => (bool)this["EnableVerboseLogging"];
        #endregion

        #region WEBDRIVER BROWSER SETTINGS

        /// <summary>
        /// Gets the setting for the URL of the page the browser will be navigated to on launch.
        /// Default = null.
        /// If not set, the browser launches with the internal startup page for the WebDriver.
        /// </summary>
        /// <remarks>
        /// Additional information when using IE:
        /// If not set, the browser launches with the internal startup page for the WebDriver server.
        /// By setting the OpenQA.Selenium.IE.InternetExplorerOptions.IntroduceInstabilityByIgnoringProtectedModeSettings to true
        /// and this property to a correct URL, you can launch IE in the Internet Protected Mode zone.
        /// This can be helpful to avoid the flakiness introduced by ignoring the Protected Mode settings.
        /// Nevertheless, setting Protected Mode zone settings to the same value in the IE configuration is the preferred method.
        /// </remarks>
        [ConfigurationProperty("InitialBrowserUrl", IsRequired = false, DefaultValue = null)]
        public string InitialBrowserUrl => (string)this["InitialBrowserUrl"];

        /// <summary>
        /// Get the PageLoadStategy for specifying the behavior of waiting for page loads in the driver.
        /// [ Default | *Normal | Eager | None ]
        /// Default = PageLoadStrategy.Normal.
        /// </summary>
        [ConfigurationProperty("PageLoadStrategy", IsRequired = false, DefaultValue = PageLoadStrategy.Normal)]
        [TypeConverter(typeof(CustomPageLoadStrategyConverter))]
        public PageLoadStrategy PageLoadStrategy => (PageLoadStrategy)this["PageLoadStrategy"];

        /// <summary>
        /// Gets the setting for the default WebDriver Wait timeout value (in seconds).
        /// Default = 3D (double).
        /// </summary>
        [ConfigurationProperty("DefaultWaitTimeout", IsRequired = false, DefaultValue = 3D)]
        [TypeConverter(typeof(DoubleConverter))]
        public double DefaultWaitTimeout => (double)this["DefaultWaitTimeout"];

        /// <summary>
        /// Gets the setting for whether to delete all cookies from the page.
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("DeleteAllCookies", IsRequired = false, DefaultValue = false)]
        public bool DeleteAllCookies => (bool)this["DeleteAllCookies"];

        /// <summary>
        /// Gets browser window IsMaximized status.
        /// This setting overrides WindowPosition and WindowSize settings.
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("IsMaximized", IsRequired = false, DefaultValue = false)]
        public bool IsMaximized => (bool)this["IsMaximized"];

        /// <summary>
        /// Gets the WebDriver browser starting window size. 
        /// Default = "1600, 900".
        /// </summary>
        [ConfigurationProperty("Size", IsRequired = false, DefaultValue = "1600, 900")]
        [TypeConverter(typeof(CustomSizeConverter))]
        public Size Size => (Size)this["Size"];

        /// <summary>
        /// Gets the WebDriver browser starting window position.
        /// Default = "10, 10".
        /// </summary>
        [ConfigurationProperty("Position", IsRequired = false, DefaultValue = "10, 10")]
        [TypeConverter(typeof(CustomPointConverter))]
        public Point Position => (Point)this["Position"];
        
        /// <summary>
        /// Gets the LogLevel setting to determine levels of logging available to WebDriver instances (usually BROWSER and DRIVER.)
        /// Default = LogLevel.Warning. (Chrome only.)
        /// [ All | Debug | Info | *Warning | Severe | Off ]
        /// </summary>
        [ConfigurationProperty("LogLevel", IsRequired = false, DefaultValue = LogLevel.Warning)]
        [TypeConverter(typeof(CustomLogLevelConverter))]
        public LogLevel LogLevel => (LogLevel)this["LogLevel"];

        #endregion

        #region CHROME ONLY SETTINGS

        /// <summary>
        /// Gets the setting for running Chrome in a headless environment. (Chrome only.)
        /// Default = false.
        /// </summary>
        [ConfigurationProperty("IsHeadless", IsRequired = false, DefaultValue = false)]
        public bool IsHeadless => (bool)this["IsHeadless"];

        /// <summary>
        /// Gets a default Chrome browser download directory. (Chrome only.)
        /// Default = null.
        /// </summary>
        [ConfigurationProperty("DownloadDefaultDir", IsRequired = false, DefaultValue = null)]
        public string DownloadDefaultDir => (string)this["DownloadDefaultDir"];
        #endregion

        #region INTERNET EXPLORERE ONLY SETTINGS

        /// <summary>
        /// Gets the setting for whether to clear the Internet Explorer cache before launching the browser. (IE only.)
        /// Default = false.
        /// Setting this to true may delay browser loading.
        /// </summary>
        [ConfigurationProperty("EnsureCleanSession", IsRequired = false, DefaultValue = false)]
        public bool EnsureCleanSession => (bool)this["EnsureCleanSession"];

        /// <summary>
        /// ADVANCED: Gets or sets a value indicating whether to ignore the settings of the Internet Explorer Protected Mode. (IE only.)
        /// Default = false.
        /// Use to fix exception when IE has mixed 'Enable Protected Mode' settings for all zones.
        /// </summary>
        [ConfigurationProperty("IgnoreProtectedModeSettings", IsRequired = false, DefaultValue = false)]
        public bool IgnoreProtectedModeSettings => (bool)this["IgnoreProtectedModeSettings"];
        #endregion
    }
}
