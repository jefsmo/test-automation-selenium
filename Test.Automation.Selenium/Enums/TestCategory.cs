using System;
using System.ComponentModel;

namespace Test.Automation.Selenium.Enums
{
    /// <summary>
    /// Represents test categories that can decorate an automated test.
    /// A test can be described using a Level, Type and Area.
    /// Example: Integration | Smoke | Web
    /// </summary>
    [Flags]
    public enum TestCategory
    {
        /// <summary>
        /// Unknown: TestCategory default value.
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        #region TEST LEVELS

        /// <summary>
        /// Level - UnitTest: verify the functionality of a specific section of code
        /// </summary>
        [Description("UnitTest")]
        UnitTest = 1,

        /// <summary>
        /// Level - Integration: verify the interfaces between components against a software design.
        /// </summary>
        [Description("Integration")]
        Integration = 2,

        /// <summary>
        /// Level - Component: check the handling of data passed between various units, or subsystem components, beyond full integration testing between those units.
        /// </summary>
        [Description("Component")]
        Component = 4,

        /// <summary>
        /// Level - System: validate the software as a whole (software, hardware, and network) against the requirements for which it was built.
        /// </summary>
        [Description("System")]
        System = 8,

        #endregion

        #region TEST TYPES

        /// <summary>
        /// Type - Smoke: minimal attempts to operate the software, designed to determine whether there are any basic problems that will prevent it from working at all.
        /// </summary>
        [Description("Smoke")]
        Smoke = 16,

        /// <summary>
        ///  Type - Functional: verify a specific action or function of the code.
        /// </summary>
        [Description("Functional")]
        Functional = 32,

        /// <summary>
        ///  Type - Accessiblity: determine if the contents of the website can be easily accessed by disable people or compliance with standards.
        /// </summary>
        [Description("Accessibility")]
        Accessibility = 64,

        /// <summary>
        /// Type - Security: degree to which a test item, and associated data and information, are protected.
        /// </summary>
        [Description("Security")]
        Security = 128,

        /// <summary>
        /// Type - AdHoc: test intended to find defects that were not found by existing test cases.
        /// </summary>
        AdHoc = 256,

        /// <summary>
        /// Type - Mock: test uses a Mock framework in place of a repository dependency.
        /// </summary>
        Mock = 512,

        #endregion

        #region TEST AREAS

        /// <summary>
        /// Area - Api: APIs are tested as per API specification.
        /// </summary>
        [Description("Api")]
        Api = 1024,

        /// <summary>
        /// Area - Database: verifies database functionality.
        /// </summary>
        [Description("Database")]
        Database = 2048,

        /// <summary>
        /// Area - Web: verifies the Web UI.
        /// </summary>
        [Description("Web")]
        Web = 4096

        #endregion
    }
}
