using Test.Automation.Selenium.Interfaces;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents an abstraction of current test data from a framework specific TestContext object.
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// Initializes a new instance of the CurrentTestData class.
        /// </summary>
        /// <param name="testContext"></param>
        /// <param name="testAttributes"></param>
        public TestData(ITestContext testContext, ITestAttributes testAttributes)
        {
            TestContext = testContext;
            TestAttributes = testAttributes;
        }

        /// <summary>
        /// Gets or sets the test context data of the current test method.
        /// </summary>
        public ITestContext TestContext { get; private set; }

        /// <summary>
        /// Gets or sets the test attributes of the current test method.
        /// </summary>
        public ITestAttributes TestAttributes { get; private set; }
    }
}
