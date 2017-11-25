using NUnit.Framework;
using Test.Automation.Selenium.Enums;

namespace Test.Automation.Selenium.NUnit
{
    //
    // Represents custom NUnit CategoryAttributes that can decorate an automated test.
    //

    /// <summary>
    /// Unknown: TestCategory default value.
    /// </summary>
    public class UnknownAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Unknown test category.
        /// </summary>
        public UnknownAttribute() : base(TestCategory.Unknown.ToDescription()) { }
    }

    #region TEST LEVELS

    /// <summary>
    /// Level - UnitTest: verify the functionality of a specific section of code
    /// </summary>
    public class UnitTestAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the UnitTest test category.
        /// </summary>
        public UnitTestAttribute() : base(TestCategory.UnitTest.ToDescription()) { }
    }

    /// <summary>
    /// Level - Integration: verify the interfaces between components against a software design.
    /// </summary>
    public class IntegrationAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Integration test category.
        /// </summary>
        public IntegrationAttribute() : base(TestCategory.Integration.ToDescription()) { }
    }

    /// <summary>
    /// Level - Component: check the handling of data passed between various units, or subsystem components, beyond full integration testing between those units.
    /// </summary>
    public class ComponentAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Component test category.
        /// </summary>
        public ComponentAttribute() : base(TestCategory.Component.ToDescription()) { }
    }

    /// <summary>
    /// Level - System: validate the software as a whole (software, hardware, and network) against the requirements for which it was built.
    /// </summary>
    public class SystemAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the System test category.
        /// </summary>
        public SystemAttribute() : base(TestCategory.System.ToDescription()) { }
    }

    #endregion

    #region TEST TYPES

    /// <summary>
    /// Type - Smoke: minimal attempts to operate the software, designed to determine whether there are any basic problems that will prevent it from working at all.
    /// </summary>
    public class SmokeAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Smoke test category.
        /// </summary>
        public SmokeAttribute() : base(TestCategory.Smoke.ToDescription()) { }
    }

    /// <summary>
    ///  Type - Functional: verify a specific action or function of the code.
    /// </summary>
    public class FunctionalAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Functional test category.
        /// </summary>
        public FunctionalAttribute() : base(TestCategory.Functional.ToDescription()) { }
    }

    /// <summary>
    ///  Type - Accessiblity: determine if the contents of the website can be easily accessed by disable people or compliance with standards.
    /// </summary>
    public class AccessibilityAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Accessiblity test category.
        /// </summary>
        public AccessibilityAttribute() : base(TestCategory.Accessibility.ToDescription()) { }
    }

    /// <summary>
    /// Type - Security: degree to which a test item, and associated data and information, are protected.
    /// </summary>
    public class SecurityAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Security test category.
        /// </summary>
        public SecurityAttribute() : base(TestCategory.Security.ToDescription()) { }
    }

    /// <summary>
    /// Type - AdHoc: test intended to find defects that were not found by existing test cases.
    /// </summary>
    public class AdHocAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the AdHoc test category.
        /// </summary>
        public AdHocAttribute() : base(TestCategory.AdHoc.ToDescription()) { }
    }


    /// <summary>
    /// Type - Mock: test uses a Mock framework in place of a repository dependency.
    /// </summary>
    public class MockAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Mock test category.
        /// </summary>
        public MockAttribute() : base(TestCategory.Mock.ToDescription()) { }
    }


    #endregion

    #region TEST AREAS

    /// <summary>
    /// Area - Api: APIs are tested as per API specification.
    /// </summary>
    public class ApiAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Api test category.
        /// </summary>
        public ApiAttribute() : base(TestCategory.Api.ToDescription()) { }
    }

    /// <summary>
    /// Area - Database: verifies database functionality.
    /// </summary>
    public class DatabaseAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Database test category.
        /// </summary>
        public DatabaseAttribute() : base(TestCategory.Database.ToDescription()) { }
    }

    /// <summary>
    /// Area - Web: verifies the Web UI.
    /// </summary>
    public class WebAttribute : CategoryAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Web test category.
        /// </summary>
        public WebAttribute() : base(TestCategory.Web.ToDescription()) { }
    }


    #endregion
}
