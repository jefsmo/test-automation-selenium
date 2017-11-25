using System.Collections.Generic;

namespace Test.Automation.Selenium.Interfaces
{
    /// <summary>
    /// Represents the attributes that decorate a test method.
    /// </summary>
    public interface ITestAttributes
    {
        /// <summary>
        /// Used to specify the description of the test. 
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Used to specify the person responsible for maintaining, running, and/or debugging the test. 
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// Used to specify the priority of a unit test. 
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Class that is used to specify the category of a unit test.
        /// </summary>
        List<string> TestCategories { get; }

        /// <summary>
        /// Establishes a test specific property on a method.
        /// </summary>
        List<KeyValuePair<string,string>> TestProperties { get; }

        /// <summary>
        /// Used to specify the time-out period of a unit test.
        /// </summary>
        int Timeout { get; }

        /// <summary>
        /// Used to specify a work item associated with a test.
        /// </summary>
        List<int> WorkItems { get; }
    }
}
