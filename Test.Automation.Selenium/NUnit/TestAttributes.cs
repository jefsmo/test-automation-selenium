using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using Test.Automation.Selenium.Interfaces;

namespace Test.Automation.Selenium.NUnit
{
    /// <summary>
    /// Represents the test attributes that decorate a test method mapped to NUnit values.
    /// </summary>
    public class TestAttributes : ITestAttributes
    {
        /// <summary>
        /// Creates an instance of the TestAttributes class.
        /// </summary>
        /// <param name="properties">The test properties provided by test framework TestContext properties.</param>
        public TestAttributes(IPropertyBag properties)
        {
            // Set the default values when the attribute is not provided.
            Priority = -1;
            Timeout = int.MaxValue;
            var testcategories = new List<string>();
            var workitems = new List<int>();
            var testproperties = new List<KeyValuePair<string, string>>();

            // Get the values when the attribute is provided.
            foreach (var key in properties.Keys)
            {
                switch (key)
                {
                    case "Description":
                        Description = (string) properties.Get("Description");
                        break;
                    case "Author":
                        Owner = (string)properties.Get("Author");
                        break;
                    case "Priority":
                        Priority = (int) properties.Get("Priority");
                        break;
                    case "Timeout":
                        Timeout = (int) properties.Get("Timeout");
                        break;
                    case "Category":
                        foreach (var item in properties["Category"])
                        {
                            testcategories.Add((string)item);
                        }
                        break;
                    case "WorkItem":
                        foreach (int item in properties["WorkItem"])
                        {
                            workitems.Add(item);
                        }
                        break;
                    default:
                        testproperties.Add(new KeyValuePair<string, string>(key, Convert.ToString(properties.Get(key))));
                        break;
                }
            }
            TestCategories = testcategories;
            WorkItems = workitems;
            TestProperties = testproperties;
        }

        /// <summary>
        /// Used to specify the description of the test. 
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Used to specify the person responsible for maintaining, running, and/or debugging the test. 
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Used to specify the priority of a unit test. 
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Class that is used to specify the category of a unit test.
        /// </summary>
        public List<string> TestCategories { get; }

        /// <summary>
        /// Establishes a test specific property on a method.
        /// </summary>
        public List<KeyValuePair<string, string>> TestProperties { get; }

        /// <summary>
        /// Used to specify the time-out period of a unit test.
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// Used to specify a work item associated with a test.
        /// </summary>
        public List<int> WorkItems { get; }
    }
}
