using NUnit.Framework;
using Shouldly;
using Test.Automation.Selenium.NUnit;

namespace UnitTestProject1
{
    [TestFixture]
    class TestClass : NUnitSeleniumBase
    {
        [Test,
            Author("Your Name"),
            Description("Verify page title is 'Bing'."),
            Property("Priority", 0),
            Web,
            Property("Some Other Property", "foo bar"),
            Property("WorkItem", 12345)]
        public void BingTitle_ShouldBeBing()
        {
            Driver.Url = Settings.BaseUri.AbsoluteUri;   // App.config setting: <LOCAL BaseUri="http://www.bing.com" />
            Driver.Title.ShouldBe("Bing", "Page title does not match expected value.");
        }
    }
}
