using System;
using System.IO;
using NUnit.Framework.Interfaces;
using Test.Automation.Selenium.Enums;
using Test.Automation.Selenium.Interfaces;
using TestContext = NUnit.Framework.TestContext;

namespace Test.Automation.Selenium.NUnit
{
    /// <summary>
    /// Represents a test framework agnostic test context derived from mapping NUnit TestContext data to generic fields.
    /// </summary>
    public class NUnitTestContext : ITestContext
    {
        /// <summary>
        /// Initializes an instance of NUnitTestContext class.
        /// </summary>
        /// <param name="testContext">The NUnit Test Context.</param>
        public NUnitTestContext(TestContext testContext)
        {
            TestName = testContext.Test.Name;
            SafeTestName = RemoveInvalidFileNameChars(testContext.Test.Name);
            FullyQualifiedTestClassName = testContext.Test.ClassName;
            BinariesDirectory = testContext.TestDirectory;
            DeploymentDirectory = testContext.TestDirectory;
            LogDirectory = testContext.WorkDirectory;
            TestResultStatus = GetTestResultStatus(testContext.Result.Outcome.Status);
            Message = testContext.Result.Message;
        }

        /// <summary>
        /// The test name from TestContext.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// The test name with any invalid file name characters replaced by a safe character.
        /// </summary>
        public string SafeTestName { get; set; }
        
        /// <summary>
        /// The fully qualified test class name from TestContext.
        /// </summary>
        public string FullyQualifiedTestClassName { get; set; }

        /// <summary>
        /// The test class short name.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// MSTest: DeploymentDirectory: Returns the directory for files deployed for the test run.
        ///  NUnit:       TestDirectory: Gets the full path of the directory containing the current test assembly.
        /// </summary>
        public string BinariesDirectory { get; set; }

        /// <summary>
        /// The current directory of the executing assembly from Directory.GetCurrentDirectory().
        /// </summary>
        public string DeploymentDirectory { get; set; }

        /// <summary>
        /// The log directory from TestContext.
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// The current test outcome from TestContext mapped to one of the enum values.
        /// [ Pass | Fail | In Progress | Not Executed | Blocked ]
        /// </summary>
        public TestResultStatus TestResultStatus { get; set; }

        /// <summary>
        /// The message from TestContext or the original test status from the test framework TestContext.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Returns the TestStatus mapped to one of the TestResultStatus values.
        /// </summary>
        /// <param name="currentTestStatus"></param>
        /// <returns></returns>
        private static TestResultStatus GetTestResultStatus(TestStatus currentTestStatus)
        {
            switch (currentTestStatus)
            {
                case TestStatus.Passed:
                    return TestResultStatus.Pass;
                case TestStatus.Failed:
                    return TestResultStatus.Fail;
                case TestStatus.Skipped:
                    return TestResultStatus.NotExecuted;
                case TestStatus.Warning:
                case TestStatus.Inconclusive:
                    return TestResultStatus.Blocked;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentTestStatus), currentTestStatus, null);
            }
        }

        /// <summary>
        /// Replaces any invalid file name characters with an 'X'.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string RemoveInvalidFileNameChars(string name)
        {
            return string.Join("X", name.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
