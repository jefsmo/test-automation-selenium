using System.ComponentModel;

namespace Test.Automation.Selenium.Enums
{
    /// <summary>
    /// Mapping of TestContext test outcomes to test framework agnostic test result status values.
    /// </summary>
    public enum TestResultStatus
    {
        /// <summary>
        /// [Ignore]:
        ///     Test is attributed in Visual Studio with IgnoreAttribute.
        /// Not Executed: 
        ///     The tester stopped the test run. 
        ///     The test that was running at the time obtains the Aborted status. 
        ///     The rest of the tests in the test run obtain the status of Not Executed.
        /// </summary>
        [Description("Not Executed")]
        NotExecuted,

        /// <summary>
        /// In Progress: 
        ///     The test is currently running.
        /// </summary>
        [Description("In Progress")]
        InProgress,

        /// <summary>
        /// Aborted:
        ///     The tester stopped the test run. 
        ///     The test that was running at the time obtains the Aborted status.
        /// Timeout:
        ///     The test or the test run timed out.
        /// Inconclusive: 
        ///     When the test ran, no Assert statement produced a Failed result, 
        ///     but at least one Assert.Inconclusive statement was satisfied. 
        ///     This result applies only to unit tests.
        /// NotRunnable:
        ///     The test could not be run because of errors in the test definition. 
        ///     For example, a unit test is NotRunnable if it returns an integer; unit test methods must return void.
        /// </summary>
        [Description("Blocked")]
        Blocked,

        /// <summary>
        /// Pass:
        ///     When the test ran, no Assert statements produced a result of Inconclusive or Failed, 
        ///     and the test did not throw an unexpected exception, and the test did not time out.
        /// </summary>
        [Description("Pass")]
        Pass,

        /// <summary>
        /// Fail:
        ///     When the test ran, at least one Assert statement produced a result of Failed, 
        ///     or the test threw an unexpected exception.
        /// </summary>
        [Description("Fail")]
        Fail
    }
}
