using System;
using System.ComponentModel;
using System.Configuration;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents the App.config EnvironmentSettings data.
    /// </summary>
    public sealed class EnvironmentSettings : ConfigurationSection
    {
        /// <summary>
        /// Gets the base URI of the Application Under Test (AUT).
        /// Default = null.
        /// </summary>
        [ConfigurationProperty("BaseUri", IsRequired = false, DefaultValue = null)]
        [Description("The base URI of the application.")]
        public Uri BaseUri => (Uri)this["BaseUri"];
    }
}
