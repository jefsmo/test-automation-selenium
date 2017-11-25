using NUnit.Framework;
using OpenQA.Selenium;
using Test.Automation.Selenium.Settings;
using TestContext = NUnit.Framework.TestContext;

namespace Test.Automation.Selenium.NUnit
{
    /// <summary>
    /// Represents an abstract base class for Selenium WebDriver test classes using NUnnit test framework.
    /// </summary>
    [TestFixture]
    public abstract class NUnitSeleniumBase
    {
        /// <summary>
        /// Gets the Selenium Context used by the base class.
        /// </summary>
        private SeleniumContext SeleniumContext { get; }

        /// <summary>
        /// Creates an instance of the NUnitSeleniumBase class.
        /// </summary>
        protected NUnitSeleniumBase()
        {
            SeleniumContext = new SeleniumContext();
        }

        /// <summary>
        /// The WebDriver instance created for each test.
        /// </summary>
        public IWebDriver Driver => SeleniumContext.Driver;

        /// <summary>
        /// EnvironmentSettings section data from App.config (via SeleniumContext.)
        /// </summary>
        public static EnvironmentSettings Settings => SeleniumContext.EnvironmentSettings;

        /// <summary>
        /// Calls SeleniumContext.StartTestRun() to intialize the DriverService used by all the tests.
        /// </summary>
        [OneTimeSetUp]
        public void NUnitBaseClassInit()
        {
            SeleniumContext.StartTestRun(TestContext.CurrentContext.TestDirectory);
        }

        /// <summary>
        /// Calls SeleniumContext.StopTestRun() to close the DriverService and remove any DriverService logs.
        /// </summary>
        [OneTimeTearDown]
        public void NUnitBaseClassCleanup()
        {
            SeleniumContext.StopTestRun(TestContext.CurrentContext.TestDirectory);
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
            SeleniumContext.StopTest(GetTestData());
        }

        /// <summary>
        /// Maps NUnit TestContext data to a test framework agnostic test data object.
        /// </summary>
        /// <returns>The populated TestData object.</returns>
        private static TestData GetTestData()
        {
            var testContext = new NUnitTestContext(TestContext.CurrentContext);
            var testAttributes = new TestAttributes(TestContext.CurrentContext.Test.Properties);

            return new TestData(testContext, testAttributes);
        }
    }
}
