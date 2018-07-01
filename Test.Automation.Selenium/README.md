# Test.Automation.Base

**README.md**

Automatically logs test attribute and test context data to the output window.  

|Test Mode|Result|Log to Output|
|---|---|---|
|Run|Pass|no|
|Run|Fail|yes|
|Debug|Pass|yes|
|Debug|Fail|yes|

 - In general, logging can add a significant performance penalty to a passing test.
 - When running hundreds of tests, this can add minutes or hours to a test run.
 - This framework only logs when tests fail or are run in debug mode.

## History
|Date|Notes|
|---|---|
|2018-07-01|Bug fixes <br> Update README|
|2018-02-18|Bug fixes|
|2017-12-05|Initial Release|

## Contents
[NUnit Test Project Workflow](#nunit-test-project-workflow)  
[Viewing Local Packages](#viewing-local-packages)  
[Octopack Reference](#octopack-reference)  
[Troubleshooting](#troubleshooting)  

## NUnit Test Project Workflow
- Ensure your test class inherits from the base class **`NUnitTestBase`**
- Ensure you have installed NUnit and NUnit3TestAdapter NuGet packages.
- If the test is run in debug mode or if the test fails, the output window displays the test log.
- If the test passes and is not run in debug mode, nothing is logged to the output window.

### Example Tests

```csharp
using NUnit.Framework;
using Test.Automation.Base;

namespace UnitTestProject1
{
    [TestFixture]
    public class UnitTest1 : NUnitTestBase
    {
        [Test]
        public void Fail_NoTestAttributes()
        {
            Assert.That(1 + 4, Is.EqualTo(4));
        }

        [Test,
            Author("Your Name"),
            Description("This test fails and has [Test] attributes."),
            Property("Priority", (int)TestPriority.High),
            Timeout(6000),
            Property("Bug", "FOO-42"),
            Property("WorkItem", 123),
            Property("WorkItem", 456),
            Property("WorkItem", 789),
            Integration, Smoke, Web]
        public void Fail_WithTestAttributes()
        {
            Assert.That(42, Is.EqualTo(41));
        }

        [Test,
            Author("Your Name"),
            Description("This test passes and has [Test] attributes."),
            Property("Priority", (int)TestPriority.Normal),
            Timeout(int.MaxValue),
            Property("ID", "BAR-42"),
            Property("WorkItem", 123),
            Property("WorkItem", 456),
            Property("WorkItem", 789),
            UnitTest, Functional, Database]
        public void Pass_WithTestAttributes()
        {
            Assert.That(42, Is.EqualTo(42));
        }

        [TestCase(2, 3, Author = "Your Name", Description = "This test fails and has [TestCase] attributes.", Category = "Integration, Smoke, Web", TestName = "Fail_WithTestCaseAttr")]
        [TestCase(-5, 9, Author = "Your Name", Category = "Integration, Smoke, Web", Description = "This test passes and has [TestCase] attributes.", TestName = "Pass_WithTestCaseAttr")]
        public void TestMulti_TestCaseAttributes(int first, int second)
        {
            Assert.That(first + second, Is.EqualTo(4));
        }
    }
}
```
### Example Test Output
#### Fail With Test Attributes
~~~text
Test Name:	Fail_WithTestAttributes
Test Outcome:	Failed
Result Message:	
Expected: 41
  But was:  42
Result StandardOutput:	
TEST ATTRIBUTES
Owner                         	Your Name                          
Description                   	This test fails and has [Test] attributes.
Timeout                       	1 (second)                         
Test Priority                 	High                               
Test Category                 	Integration, Smoke, Web            
Test Property                 	[Bug, FOO-42]                      
Work Item                     	123, 456, 789                      
================================================================================
TEST CONTEXT
Unique ID                     	0-1002                             
Class Name                    	UnitTestProject1.UnitTest1         
Method Name                   	Fail_WithTestAttributes            
Test Name                     	Fail_WithTestAttributes            
Binaries Dir                  	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
Deployment Dir                	C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE
Logs Dir                      	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
================================================================================
~~~

#### Pass With Test Attributes (In Debug Mode)
~~~text
Test Name:	Pass_WithTestAttributes
Test Outcome:	Passed
Result StandardOutput:	
TEST ATTRIBUTES
Owner                         	Your Name                          
Description                   	This test passes and has [Test] attributes.
Timeout                       	Infinite                           
Test Priority                 	Normal                             
Test Category                 	UnitTest, Functional, Database     
Test Property                 	[ID, BAR-42]                       
Work Item                     	123, 456, 789                      
================================================================================
TEST CONTEXT
Unique ID                     	0-1003                             
Class Name                    	UnitTestProject1.UnitTest1         
Method Name                   	Pass_WithTestAttributes            
Test Name                     	Pass_WithTestAttributes            
Binaries Dir                  	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
Deployment Dir                	C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE
Logs Dir                      	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
================================================================================
~~~

#### Fail With No Test Attributes
~~~text
Test Name:	Fail_NoTestAttributes
Test Outcome:	Failed
Result Message:	
Expected: 4
  But was:  5
Result StandardOutput:	
TEST ATTRIBUTES
Owner                         	Unknown                            
Description                   	Unknown                            
Timeout                       	Infinite                           
Test Priority                 	Unknown                            
Test Category                 	Unknown                            
Test Property                 	[Unknown, Unknown]                 
Work Item                     	Unknown                            
================================================================================
TEST CONTEXT
Unique ID                     	0-1001                             
Class Name                    	UnitTestProject1.UnitTest1         
Method Name                   	Fail_NoTestAttributes              
Test Name                     	Fail_NoTestAttributes              
Binaries Dir                  	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
Deployment Dir                	C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE
Logs Dir                      	C:\Source\Repos\test-automation-base\UnitTestProject1\bin\Debug
================================================================================
~~~

## Viewing Local Packages
- Install NuGet Package Explorer to view local packages.  
- [NuGetPackageExplorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)

## Octopack Reference
#### Create a Local NuGet Package with OctoPack
- Add a `.nuspec` file to each project in the solution that you want to package with NuGet.
- The `.nuspec` file name **must be the same name as the project** with the `.nuspec` extension
- Open a '`Developer Command Prompt for VS2017`' command window.
- Navigate to the solution or project that you want to OctoPack.
- Run the following command:
#### MSBuild Octopack Command

```text
MSBUILD Test.Automation.Base.csproj /t:Rebuild /p:Configuration=Release /p:RunOctoPack=true /p:OctoPackPublishPackageToFileShare=C:\Packages /p:OctoPackPackageVersion=1.0.0 /fl
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

## Troubleshooting
TBD
