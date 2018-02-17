using NUnit.Framework;
using OpenQA.Selenium;
using Test.Automation.Base;
using Test.Automation.Selenium.Settings;

namespace Test.Automation.Selenium.NUnit
{
    /// <summary>
    /// Represents an abstract base class for Selenium WebDriver test classes using NUnnit test framework.
    /// </summary>
    [TestFixture]
    public abstract class NUnitSeleniumBase : NUnitTestBase
    {
        /// <summary>
        /// Gets or sets the Selenium Context used by the base class.
        /// </summary>
        private SeleniumContext SeleniumContext { get; set; }

        /// <summary>
        /// Gets the WebDriver instance created for each test.
        /// </summary>
        protected IWebDriver Driver { get; set; }

        /// <summary>
        /// Gets EnvironmentSettings section data from App.config.
        /// </summary>
        protected static EnvironmentSettings Settings { get; set; }

        /// <summary>
        /// Creates an instance of the NUnitSeleniumBase class.
        /// </summary>
        public NUnitSeleniumBase()
        {
            SeleniumContext = new SeleniumContext();
            Driver = SeleniumContext.Driver;
            Settings = SeleniumContext.EnvironmentSettings;
        }
        
        /// <summary>
        /// Calls SeleniumContext.StartTestRun() to intialize the DriverService used by all the tests.
        /// </summary>
        [OneTimeSetUp]
        public void NUnitBaseClassInit()
        {
            SeleniumContext.StartTestRun(MappedContext.TestBinariesDirectory);
        }

        /// <summary>
        /// Calls SeleniumContext.StopTestRun() to close the DriverService and remove any DriverService logs.
        /// </summary>
        [OneTimeTearDown]
        public void NUnitBaseClassCleanup()
        {
            SeleniumContext.StopTestRun(MappedContext.TestBinariesDirectory);
        }

        /// <summary>
        /// Calls SeleniumContext.StartTest() to create a new WebDriver instance.
        /// </summary>
        [SetUp]
        public void NUnitBaseTestInit()
        {
            SeleniumContext.StartTest();
        }

        /// <summary>
        /// Calls SeleniumContext.StopTest() to close the WebDriver instance and log test repro info upon failure.
        /// </summary>
        [TearDown]
        public void NUnitBaseTestCleanup()
        {
            SeleniumContext.StopTest(MappedContext);
        }
    }
}
