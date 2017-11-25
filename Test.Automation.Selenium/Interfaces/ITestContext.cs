using Test.Automation.Selenium.Enums;

namespace Test.Automation.Selenium.Interfaces
{
    /// <summary>
    /// Represents an interface for a test framework agnostic test context.
    /// </summary>
    public interface ITestContext
    {
        /// <summary>
        /// The test name from TestContext.
        /// </summary>
        string TestName { get; set; }

        /// <summary>
        /// The test name with any invalid file name characters replaced by a safe character.
        /// </summary>
        string SafeTestName { get; set; }

        /// <summary>
        /// The fully qualified test class name from TestContext.
        /// </summary>
        string FullyQualifiedTestClassName { get; set; }

        /// <summary>
        /// The test class short name.
        /// </summary>
        string ClassName { get; set; }

        /// <summary>
        /// VS TEST: DeploymentDirectory: Returns the directory for files deployed for the test run.
        /// NUNIT:   TestDirectory:       Gets the full path of the directory containing the current test assembly.
        /// </summary>
        string BinariesDirectory { get; set; }

        /// <summary>
        /// The current directory of the executing assembly from Directory.GetCurrentDirectory().
        /// </summary>
        string DeploymentDirectory { get; set; }

        /// <summary>
        /// The log directory from TestContext.
        /// </summary>
        string LogDirectory { get; set; }

        /// <summary>
        /// The current test outcome from TestContext mapped to one of the enum values.
        /// [ Pass | Fail | In Progress | Not Executed | Blocked ]
        /// </summary>
        TestResultStatus TestResultStatus { get; set; }

        /// <summary>
        /// The message from TestContext or the original test status from the test framework TestContext.
        /// </summary>
        string Message { get; set; }
    }
}
