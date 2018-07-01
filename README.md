# Test Automation Selenium

**README.md**

## History
|Date|Notes|
|---|---|---|
|2018-06-29|Fix IE bugs by adding 'IntroduceInstabilityByIgnoringProtectedModeSettings' arg |
|2018-02-19|Fix chromedriver memory leak by removing 'no-sandbox' arg|
|2018-02-19|Bug fixes|
|2017-12-05|Initial Release|

## Contents
[Test.Automation Projects Overview](#test.automation-projects-overview)  
[Test.Automation.Base](#test.automation.base)  
[Test.Automation.Selenium](#test.automation.selenium)  
[Test.Automation.Data](#test.automation.data)  
[Test.Automation.Api](#test.automation.api)  
[Test.Automation WebDriver Projects](#test.automation-webdriver-projects)  
[WebDriver Download Links](#webdriver-download-links)  
[NUnit Test Framework Workflow](#nunit-test-framework-workflow)  
[WebDriver Test Examples](#webdriver-test-examples)  
[Debug Mode Output](#debug-mode-output)  
[Example App.Config File](#example-app.config-file)  
[TroubleShooting](#troubleshooting)  
[Creating Packages Locally](#creating-packages-locally)  

## Test.Automation Projects Overview
The Test.Automation.* projects create NuGet packages for test automation.  
The NuGet packages can be generated locally to a packages folder on your hard drive.  
**`> C:\Packages`**

Once you have created the packages locally, use Visual Studio NuGet package manager to create a local package source.  
Add packages to your test project with NuGet package manager pointing to the local package source.

### Test.Automation.Base
This project provides common base class logging methods for test automation test classes.  
The test attributes and test context directory locations are logged to the console window if the test does not pass or is run in debug mode.

The base class is:  
   - **TestAutomationBase**

### Test.Automation.Selenium
This project creates a NuGet package that enables Selenium WebDriver tests.  
The package contains a base class for the NUnit test framework.  
Once a test class inherits from the base class, it just needs to pass a URL to the WebDriver to start a Selenium test.  
- The base class handles WebDriver creation and disposal.  
- Each test gets a new WebDriver instance.  
- Each test also logs test repro data when a test fails or is run in debug mode.  

The base class is:
- **NUnitSeleniumBase**

### Test.Automation.Data
This project creates a NuGet package containing a **SqlHelper** class that tests can use to retrieve data from a SQL database.  
SqlHelper handles all the connection, command, and reader resources automatically.
Write an SQL statement or call a stored procedure and pass the sql string to one of the execute methods:
- ExecuteDataTable
- ExecuteReader
- ExecuteScalar
- ExecuteNonQuery

In addition, it provides the **Common** class that contains common SQL queries used for database BVT tests.
This includes queries for users, user roles and schema objects.

### Test.Automation.Api
This project creates an API base class that can be used in a project to create methods that call JSON REST API endpoints.  
Call the ExecuteAsync() method by passing in an HTTP Request.

### Test.Automation WebDriver Projects
The WebDriver projects create NuGet packages for browser spcecific WebDrivers.  
Each test project has the appropriate WebDriver copied to the binaries directory when the test is run.  
You can update the WebDriver to a current version by copying the new version to the project and regenerating the NuGet package.  

Each WebDriver is related to a specific browser.  
You need a WebDriver for each browser you intend to run tests against.  

The solution includes the following WebDriver projects:  

|Driver|Browser|Project|
|----|----|----|
|ChromeDriver|Chrome|Test.Automation.ChromeDriver|
|Headless Chrome|Chrome in headless mode|Use ChromeDriver <br> set IsHeadless="true"|
|IEDriverServer|Internet Explorer|Test.Automation.IEDriverServer|
|MicrosoftWebDriver|Edge browser|Test.Automation.MicrosoftWebDriver|

#### WebDriver Download Links
**README.WebDriver.md**

|Browser | WebDriver | Download |Notes|
|-----|-----|-----|----|
| [Chrome](https://www.google.com/chrome/browser/desktop/index.html?system=true&standalone=1) | chromedriver.exe | [ChromeDriver](https://sites.google.com/a/chromium.org/chromedriver/downloads) | [Wiki](https://github.com/SeleniumHQ/selenium/wiki/ChromeDriver) <br> Use Chrome's 'alternative installer'<br>Recommended for testing Web applications built with moden javascript frameworks like React and Ember |
| HeadLess Chrome | chromedriver.exe | see above | Headless web testing<br>Set **`IsHeadless="true"`** in App.config |
| [Internet Explorer (IE)](https://support.microsoft.com/en-us/help/17621/internet-explorer-downloads) | IEDriverServer.exe | [IEDriverServer](http://selenium-release.storage.googleapis.com/index.html)  | [Required Configuration](https://github.com/SeleniumHQ/selenium/wiki/InternetExplorerDriver#required-configuration) <br> Windows default browser prior to Windows 10 |
| [Microsoft Edge](https://www.microsoft.com/en-us/windows/microsoft-edge#AoPhgFHFcSwpqU6Z.97) | MicrosoftWebDriver.exe | [MicrosoftWebDriver](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/)| [WebDriver](https://docs.microsoft.com/en-us/microsoft-edge/webdriver) <br> Windows 10 default browser <br> Will be a Feature on Demand in a future Windows version |

## NUnit Test Framework Workflow
- Create a Visual Studio Test Project using the VS test project template.
  - The test project will have the test icon.
- Open NuGet Package Manager and remove the two MSTest packages.
- Delete the test class added by the template. 
- Add **Test.Automation.Selenium** NuGet package to the test project.
- Add a test class to the test project.
- Add **`[TestFixture]`** attribute to the test class.
- Ensure the test class inherits from the **NUnitSeleniumBase**.
   - **`public class UnitTest1 : NUnitSeleniumBase`**  
- Add a test method attribute to the method.
   - Add **`[Test]`** attribute to make it a test method.
- To start an instance of the WebDriver in your test, you must assign an URL.
   - **`Driver.Url = "http://www.bing.com";`**
   - You can configure a base URL for your tests in the App.config file
- Edit the App.config file in your test project to customize WebDriver settings. 
- Run your test:
  - If the test fails (or is run in debug mode) you can find debugging information in the output window.
  - A screenshot is automatically taken for failed tests (and in debug mode.)
  - WebDriver logs are automatically created for failed tests (and in debug mode.)
  - By default, **no output** is written for tests that pass.


### WebDriver Test Examples
- To use WebDriver, simply reference 'Driver' in the test.  

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
### Debug Mode Output

~~~text
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
~~~

### Example App.config File
An App.config file should be installed by the Test.Automation.Selenium package.  
You can use this file as a reference if the original file is deleted or broken.  

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
    *-Denotes default value. Settings are optional unless noted otherwise.
    Name                      WebDriver browser name.                           [ *Chrome | IE | MicrosoftEdge ]
    Position                  Browser window position. (from upper left corner) [ *(10, 10) ]
    Size                      Browser window size. (width, height)              [ *(1600, 900) ] 
    IsMaximized               Maximize browser window. (overrides window size)  [ true | *false ]
    HideCommandPromptWindow   Hide WebDriver service command window.            [ *true | false ] 
    DefaultWaitTimeout        Default WebDriver Wait timeout value. (seconds)   [ *3 ]
    DownloadDefaultDir        Default Chrome download directory                 (Chrome only)
    IsHeadless                Run the Chrome browser in a headless environment  (Chrome only)[ true | *false] 
    EnableVerboseLogging      Enable DriverService verbose logging.             [ true | *false ] 
    LogLevel                  Level of logging for WebDriver instances.         [ All | Debug | Info | *Warning | Severe | Off ]
    IntroduceInstabilityByIgnoringProtectedModeSettings Ignore IE Protected Mode settings (IE only) [ true | *false ]
    ===========================================================================
    -->
    <TEST Name="Chrome" />
    <LOCAL Name="Chrome" />
  </browserSettings>

</configuration>

```

## TroubleShooting
#### No MicrosoftWebDriver logs
~~~text
UPDATE: Console logging will be supported in a future version of Microsoft Web Driver.
[Platform Status](https://developer.microsoft.com/en-us/microsoft-edge/platform/status/consoleloggingapis/?q=log)  

First, the JsonWireProtocol as defined by SeleniumHQ is deprecated.
It was in this protocol specification that the logging capabilities for the WebDriver session were defined.

“Status: OBSOLETE See current W3C specification for WebDriver”

The current WebDriver Recommendation does not have logging capabilities defined.
I do not know why these were dropped.
Perhaps these logs were thought best collected at each source?
~~~

## Creating Packages Locally
### OctoPack Command Line Reference
#### Create a Local NuGet Package with OctoPack
- Add a `.nuspec` file to each project in the solution that you want to package with NuGet.
- The `.nuspec` file name **must be the same name as the project** with the `.nuspec` extension
- Open a '`Developer Command Prompt for VS2017`' command window.
- Navigate to the solution or project that you want to OctoPack.
- Run the following command:

```text
// To Create packages for each project in the solution:
MSBUILD Test.Automation.Selenium.sln /t:Rebuild /p:Configuration=Release /p:RunOctoPack=true /p:OctoPackPackageVersion=1.0.0 /p:OctoPackPublishPackageToFileShare=C:\Packages

// To Create a package for a single project:
MSBUILD Test.Automation.Selenium.csproj /t:Rebuild /p:Configuration=Release /p:RunOctoPack=true /p:OctoPackPackageVersion=1.0.0 /p:OctoPackPublishPackageToFileShare=C:\Packages
```

#### MSBUILD OctoPack Command Syntax
|Switch|Value|Definition|
|-----|-----|-----|
|`/t:Rebuild`|  |MSBUILD Rebuilds the project(s).|
|`/p:Configuration=`|`Release`|Creates and packages a Release build.|
|`/p:RunOctoPack=`|`true`|Creates packages with Octopack using the .nuspec file layout.|
|`/p:OctoPackPackageVersion=`|`1.0.0`|Updates Package Version.|
|`/p:OctoPackPublishPackageToFileShare=`|`C:\Packages`|Copies packages to local file location.|
    
#### Other OctoPack Options:

|Switch|Value|Description|
|-----|-----|-----|
|`/p:Configuration=`|`[ Release | Debug ]`|The build configuration|
|`/p:RunOctoPack=`|`[ true | false ]`|Enable or Disable OctoPack|
|`/p:OctoPackPackageVersion=`|`1.2.3`|Version number of the NuGet package. By default, OctoPack gets the version from your assembly version attributes. Set this parameter to use an explicit version number.|
|`/p:OctoPackPublishPackageToFileShare=`|`C:\Packages`|Copies packages to the specified directory.|
|`/p:OctoPackPublishPackageToHttp=`|`http://my-nuget-server/api/v2/package`| Pushes the package to the NuGet server|
|`/p:OctoPackPublishApiKey=`|`ABCDEFGMYAPIKEY`|API key to use when publishing|
|`/p:OctoPackNuGetArguments=`| `-Verbosity detailed`|Use this parameter to specify additional command line parameters that will be passed to NuGet.exe pack.|
|`/p:OctoPackNuGetExePath=`|`C:\MyNuGetPath\`|OctoPack comes with a bundled version of NuGet.exe. Use this parameter to force OctoPack to use a different NuGet.exe instead.|
|`/p:OctoPackNuSpecFileName=`|`<C#/VB_ProjectName>.nuspec`|The NuSpec file to use.|

### Viewing Local Packages
- Install NuGet Package Explorer to view local packages.  
- [NuGetPackageExplorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)

### NuGet Command Line Reference
Ensure that NuGet.exe is in your path.  
Running NuGet using a .nuspec file allows greater control over what files are packed and excluded.
- Add a `.nuspec` file to each project in the solution that you want to package with NuGet.
- The `.nuspec` file name **must be the same name as the project** with the `.nuspec` extension

```text
NUGET PACK Test.Automation.Selenium.nuspec -Verbosity detailed  -OutputDirectory "C:\Packages"
```

##### Explaination of Command Arguments
|Argument|Value|Description|
|----|----|----|
|`PACK`|`Test.Automation.Selenium.nuspec`|Create a package from .nuspec file|
|`-Verbosity`|`detailed`|Displays verbose command output for debugging|
|`-OutputDirectory`|`"C:\Packages"`|Copies package to local directory|

