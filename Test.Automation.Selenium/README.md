# Test.Automation.Base
**README.md**
## History
|Date|Notes|
|---|---|
|2018-02-18|Bug fixes|
|2017-12-05|Initial Release|

## Contents
[NUnit Test Project Workflow](#nunit-test-project-workflow)  
[Octopack Reference](#octopack-reference)  
[Viewing Local Packages](#viewing-local-packages)  
[Troubleshooting](#troubleshooting)  

## NUnit Test Project Workflow

## Examples

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


## Octopack Reference
#### Create a Local NuGet Package with OctoPack
- Add a `.nuspec` file to each project in the solution that you want to package with NuGet.
- The `.nuspec` file name **must be the same name as the project** with the `.nuspec` extension
- Open a '`Developer Command Prompt for VS2017`' command window.
- Navigate to the solution or project that you want to OctoPack.
- Run the following command:
#### MSBuild Octopack Command

```
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

## Viewing Local Packages
- Install NuGet Package Explorer to view local packages.  
- [NuGetPackageExplorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)

## Troubleshooting
