# Test.Automation.Selenium
**README.Selenium.md**
## Contents
[Verify App.config File Created](#verify-app-config-file-created)  
[Install and Update NuGet Packages ](#install-and-update-nuget-packages)  
[NUnit Test Framework Workflow ](#nunit-test-framework-workflow)  
[NUnitSeleniumBase Example](#nunitseleniumbase-example)  
[Example App Config File](#example-app-config-file)  

## Verify App Config File Created
This package should add an App.config file and modify it with custom configuration sections.  
**Verify the App.config file is created.**

If the App.config file is misssing, add it using Add | New Item... | General | Application Configuration File  
Copy/Paste the content from the example App.config file below.

The supplied BrowserSettings work well for development (LOCAL) and running on a test machine (TEST).  

Use the `<appSettings>` **"testRunSetting"** to switch between test environments easily.
```
  <appSettings>
    <!--
    testRunSetting = Choose a test environment. [ LOCAL | TEST | < ... >  ]
    ===========================================================================
    -->
    <add key="testRunSetting" value="TEST" />
  </appSettings>
```

## Install and Update NuGet Packages
WebDriver driver packages and Selenium are updated frequently.  
Check for updates using the NuGet package manager in Visual Studio.  

You may want to add other NuGet packages useful for WebDriver tests:    
- **ChromeDriver** is installed with the package.
   - At least one WebDriver package **must be installed** in your test project.
- You may want to install other WebDrivers:
     - **IEDriverServer** is used for Internet Explorer
   - Ensure that the browser for your WebDriver is already installed on the machine used for testing.  
- **Selenium.Support** is installed with the package.
   - For creating Page Objects, Page Factory objects and WebDriverWait expected conditions.  
- **Shouldly**  is installed with the package.
   - For fluent test assertions.  

## NUnit Test Framework Workflow
- Create a Visual Studio Test Project using the VS test project template.
  - The test project will have the test icon.
- Open NuGet Package Manager and remove the two MSTest packages.
- Delete the test class added by the template. 
- Add **Test.Automation.Selenium** NuGet package to the test project.
- Add a test class to the test project.
- Add `[TestFixture]` attribute to the test class.
- Ensure the test class inherits from the **NUnitSeleniumBase**.
   - `public class UnitTest1 : NUnitSeleniumBase`  
- Add a test method attribute to the method.
   - Add `[Test]` attribute to make it a test method.
- To start an instance of the WebDriver in your test, you must assign an URL.
   - `Driver.Url = "http://www.bing.com"; `
   - You can configure a base URL for your tests in the App.config file
- Edit the App.config file in your test project to customize WebDriver settings. 
- Run your test:
  - If the test fails (or is run in debug mode) you can find debugging information in the output window.
    - A screenshot is automatically taken for failed tests (and in debug mode.)
    - WebDriver logs are automatically created for failed tests (and in debug mode.)
  - By default, **no output** is written for tests that pass.

## NUnitSeleniumBase Example
```
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
            Driver.Url = Settings.BaseUri.AbsoluteUri;   // <LOCAL BaseUri="http://www.bing.com" />
            Driver.Title.ShouldBe("Bing", "Page title does not match expected value.");
        }
    }
}
```
### Debug Mode Output
![BingTitle_ShouldBeBing.png](./BingTitle_ShouldBeBing.png)


## Example App Config File
An App.config file should be installed by the Test.Automation.Selenium package.  
You can use this file as a reference if the original file is deleted or broken.  

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>

  <configSections>
    <sectionGroup name="environmentSettings">
      <section name="LOCAL" type="Test.Automation.Selenium.Settings.EnvironmentSettings, Test.Automation.Selenium" />
      <section name="TEST" type="Test.Automation.Selenium.Settings.EnvironmentSettings, Test.Automation.Selenium" />
    </sectionGroup>
    <sectionGroup name="browserSettings">
      <section name="LOCAL" type="Test.Automation.Selenium.Settings.BrowserSettings, Test.Automation.Selenium" />
      <section name="TEST" type="Test.Automation.Selenium.Settings.BrowserSettings, Test.Automation.Selenium" />
    </sectionGroup>
  </configSections>

  <appSettings>
    <!--
    testRunSetting = Choose a test environment. [ LOCAL | TEST | < ... > ]
    ===========================================================================
    -->
    <add key="testRunSetting" value="TEST" />
  </appSettings>

  <connectionStrings>
    <clear />
    <add name="LOCAL" providerName="System.Data.SqlClient" connectionString="Data Source=MyLocalSqlServer;" />
    <add name="TEST" providerName="System.Data.SqlClient" connectionString="Data Source=MyLocalSqlServer;" />
  </connectionStrings>

  <environmentSettings>
    <!-- 
    BaseUri = Base URI of Application Under Test (AUT).
    ===========================================================================
    -->
    <LOCAL BaseUri="http://www.bing.com" />
    <TEST BaseUri="http://www.bing.com" />
  </environmentSettings>

  <browserSettings>
    <!--
    *-Denotes default value. Settings are optional unless noted otherwise.
    Name                    =  WebDriver browser name.                           [ *Chrome | IE | Edge ]
    Position                =  Browser window position. (from upper left corner) [ *(10, 10) ]
    Size                    =  Browser window size. (width, height)              [ *(1600, 900) ] 
    IsMaximized             =  Maximize browser window. (overrides window size)  [ true | *false ]
    HideCommandPromptWindow =  Hide WebDriver service command window.            [ *true | false ] 
    DefaultWaitTimeout      =  Default WebDriver Wait timeout value. (seconds)   [ *3 ]
    DownloadDefaultDir      =  Default Chrome download directory                 (Chrome only)
    IsHeadless              =  Run the Chrome browser in a headless environment  (Chrome only)[ true | *false] 
    ===========================================================================
    -->
    <LOCAL Name="Chrome" />
    <TEST Name="Chrome"  />
  </browserSettings>

</configuration>

```  
