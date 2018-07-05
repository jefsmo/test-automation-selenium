# Test.Automation.Selenium

**README.Selenium.md**

## Contents
[Verify App.config File Created](#verify-app-config-file-created)  
[Install and Update NuGet Packages ](#install-and-update-nuget-packages)  
[NUnit Test Framework Workflow ](#nunit-test-framework-workflow)  
[NUnitSeleniumBase Examples](#nunitseleniumbase-examples)  
[Example App Config File](#example-app-config-file)  

## Verify App Config File Created
This package should add an App.config file and modify it with custom configuration sections.  

If the App.config file is misssing, add it in Visual Studio using `Add | New Item... | General | Application Configuration File`  
Copy/Paste the content from the example App.config file below.

The supplied BrowserSettings work well for development (LOCAL) and running on a test machine (TEST).  

Use the `<appSettings>` **"testRunSetting"** to switch between test environments easily.

```xml
  <appSettings>
    <add key="testRunSetting" value="TEST" />
  </appSettings>
```

## Install and Update NuGet Packages
WebDriver driver packages and Selenium are updated frequently.  
Check for updates using the NuGet package manager in Visual Studio.  

You may want to add other NuGet packages useful for WebDriver tests:
- At least one WebDriver package **must be installed** in your test project.
  - **ChromeDriver** is installed with the package.
  - **IEDriverServer** is used for Internet Explorer
  - **MicrosoftWebDriver** is for Edge browser on Windows 10
- Ensure that the browser for your WebDriver is already installed on the machine used for testing.  
  - Chrome must be installed to use chromedriver etc.

## NUnit Test Framework Workflow
- Create a Visual Studio Test Project using the VS test project template.
  - The test project will have the test icon.
- Open NuGet Package Manager and remove the two MSTest packages.
- Delete the test class added by the template. 
- Add **Test.Automation.Selenium** NuGet package to the test project.
- Add a test class to the test project.
- Add `[TestFixture]` attribute to the test class.
- Ensure the test class inherits from the **NUnitSeleniumBase**.
   - **`public class UnitTest1 : NUnitSeleniumBase`** 
- Add a test method attribute to the method.
   - Add `[Test]` attribute to make it a test method.
- To start an instance of the WebDriver in your test, you must assign an URL.
   - **`Driver.Url = "https://www.bing.com";`** 
   - You can configure a base URL for your tests in the App.config file
- Edit the App.config file in your test project to customize WebDriver settings. 
- Run your test:
  - If the test fails (or is run in debug mode) you can find debugging information in the output window.
    - A screenshot is automatically taken for failed tests (and in debug mode.)
    - WebDriver logs are automatically created for failed tests (and in debug mode.)
  - By default, **no output** is written for tests that pass.

## NUnitSeleniumBase Examples
```csharp
using System;
using NUnit.Framework;
using Test.Automation.Base;
using Test.Automation.Selenium;

namespace UnitTestProject1
{
    [TestFixture]
    public class UnitTest1 : NUnitSeleniumBase
    {
        [Test]
        public void TestMethod1()
        {
            Driver.Url = "https://www.bing.com";
            Assert.That(Driver.Title, Is.EqualTo("Bing"));
        }

        [Test,
            Author("Your Name"),
            Description("Verify page title is 'Bing'."),
            Property("Priority", 0),
            Web,
            Property("Some Other Property", "foo bar"),
            Property("WorkItem", 12345)]
        public void Bing_Title_ShouldBeBing()
        {
            Driver.Url = Settings.BaseUri.AbsoluteUri; 
            Assert.That(Driver.Title, Is.EqualTo("Bing"), "Page title does not match expected value.");
        }
    }
}
```

## Example Test Output - Run

~~~text
Test Name:	Bing_Title_ShouldBeBing
Test FullName:	UnitTestProject1.UnitTest1.Bing_Title_ShouldBeBing
Test Source:	C:\Source\Repos\test-automation-selenium\UnitTestProject1\UnitTest1.cs : line 25
Test Outcome:	Passed
Test Duration:	0:00:05.745
~~~

## Example Test Output - Debug
```text
Test Name:	Bing_Title_ShouldBeBing
Test Outcome:	Passed
Result StandardOutput:	
ENVIRONMENT SETTINGS
Run Environment          	TEST                          
Base URI                 	https://www.bing.com/         
================================================================================
WEBDRIVER SERVICE SETTINGS
Service Name             	OpenQA.Selenium.IE.InternetExplorerDriverService
Service State            	RUNNING                       
Service Window           	HIDDEN                        
Service URI              	http://localhost:5555/        
Service Port             	5555                          
Process ID               	10748                         
================================================================================
WEBDRIVER BROWSER SETTINGS
Browser Name             	IE                            
Browser Window           	NORMAL                        
Browser Mode             	NORMAL                        
Browser Size             	{Width=800, Height=600}       
Browser Position         	{X=10,Y=10}                   
================================================================================
BROWSER STATE
Browser Caps             	Capabilities [BrowserName=internet explorer, Platform=Any, Version=]
Browser URL              	https://www.bing.com/         
Browser Title            	Bing                          
================================================================================
WebDriver Logs not available for 'internet explorer' WebDriver; EXCEPTION: Object reference not set to an instance of an object.
TEST ATTRIBUTES
Owner                    	Your Name                     
Description              	Verify page title is 'Bing'.  
Timeout                  	Infinite                      
Test Priority            	Unknown                       
Test Category            	Web                           
Test Property            	[Some Other Property, foo bar]
Work Item                	12345                         
================================================================================
TEST CONTEXT
Unique ID                	0-1002                        
Class Name               	UnitTestProject1.UnitTest1    
Method Name              	Bing_Title_ShouldBeBing       
Test Name                	Bing_Title_ShouldBeBing       
Binaries Dir             	C:\Source\Repos\test-automation-selenium\UnitTestProject1\bin\Debug
Deployment Dir           	C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE
Logs Dir                 	C:\Source\Repos\test-automation-selenium\UnitTestProject1\bin\Debug
================================================================================
```

## Example App Config File
- An App.config file should be installed by the Test.Automation.Selenium package.  
- You can use this file as a reference if the original file is deleted or broken.  

```xml
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
    <add name="TEST.localhost" providerName="System.Data.SqlClient" connectionString="Data Source=localhost;" />
  </connectionStrings>

  <environmentSettings>
    <!-- 
    BaseUri = Base URI of Application Under Test (AUT).
    ===========================================================================
    -->
    <TEST BaseUri="https://www.bing.com" />
    <LOCAL BaseUri="https://www.bing.com" />
  </environmentSettings>

  <browserSettings>
    <!--
    All SETTINGS OPTIONAL (unless noted otherwise.)
    *-Denotes default value. 
    === ALL BROWSERS ==========================================================
    Name                        WebDriver browser name.                            [ *Chrome | IE | MicrosoftEdge ]
    HideCommandPromptWindow     Hide command prompt window of the service.         [ *true | false ]
    InitialBrowserUrl           URL browser will be navigated to on launch.        [ *null | (https://www.bing.com) ]
    DefaultWaitTimeout          Default WebDriver Wait timeout value. (seconds)    [ *3 ]
    PageLoadStrategy            Specifies the behavior of waiting for page loads.  [ Default | *Normal | Eager | None ]
    IsMaximized                 Maximize browser window. (overrides size/position) [ true | *false ]
    Size                        Browser window size. (width, height)               [ *(1600, 900) ]
    Position                    Browser window position. (from upper left corner)  [ *(10, 10) ]
    EnableVerboseLogging        Enable DriverService verbose logging.              [ true | *false ]
    LogLevel                    Level of logging for WebDriver instance logs.      [ All | Debug | Info | *Warning | Severe | Off ]
    === CHROME ONLY ===         ===============================================
    IsHeadless                  Run Chrome browser in a headless environment.      [ true | *false] 
    DownloadDefaultDir          Default Chrome download directory.                 [ *null | (C:\MyDownloadDefaultDir) ]
    === INTERNET EXPLORER ONLY  ===============================================
    EnsureCleanSession          Clear the IE cache before launching the browser.   [ true | *false]
    IgnoreProtectedModeSettings Ignore mixed IE Protected Mode zone settings.      [ true | *false ]
    ===========================================================================
    -->
    <TEST Name="Chrome"  />
    <LOCAL Name="Chrome" />
  </browserSettings>

</configuration>
```  
