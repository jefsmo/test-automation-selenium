using System;
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
        /// </summary>
        [ConfigurationProperty("BaseUri", IsRequired = false)]
        public Uri BaseUri => (Uri)this["BaseUri"];
    }
}
