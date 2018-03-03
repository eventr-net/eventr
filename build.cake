#load "utils.cake"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=xunit.runner.console"

const string Clean = "Clean";
const string Restore = "Restore";
const string Compile = "Compile";
const string Build = "Build";
const string Test = "Test";
const string Analyze = "Analyze";
const string Pack = "Pack";
const string Publish = "Publish";

const string Src = "./src";
const string TestSrc = "./test";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("Target", Build);
var configuration = Argument("Configuration", "Release");
var artifactsPath = Argument("Artifacts", "./artifacts");
var testResultsPath = Argument("TestResults", "./test-results");
var html = bool.Parse(Argument("Html", "true"));


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = GetSlnFile(".").FullPath;
var semVer = GetSemanticVersion();
var buildNumber = GetBuildNumber();

Information("{0} build {1} ({2})", semVer, buildNumber, configuration);


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task(Clean)  
    .Does(() =>
{
    var settings = new DeleteDirectorySettings { Recursive = true, Force = true };
    if (DirectoryExists(artifactsPath)) DeleteDirectory(artifactsPath, settings);  
    if (DirectoryExists(testResultsPath)) DeleteDirectory(testResultsPath, settings);
    DeleteDirectories(GetDirectories(string.Format("{0}/**/bin", Src)), settings);
    DeleteDirectories(GetDirectories(string.Format("{0}/**/obj", Src)), settings);
});


Task(Restore)  
    .Does(() =>
{
    DotNetCoreRestore(solutionPath);
});


Task(Compile)
    .Does(() =>
{
    var buildSettings = new DotNetCoreBuildSettings 
    { 
        Configuration = configuration,
        NoRestore = true,
        Verbosity = DotNetCoreVerbosity.Minimal,
        MSBuildSettings = new DotNetCoreMSBuildSettings
        {
            NoLogo = true
        }
    };
    DotNetCoreBuild(solutionPath, buildSettings);
});


Task(Build)
    .IsDependentOn(Clean)
    .IsDependentOn(Restore)
    .IsDependentOn(Compile)
    .Does(() =>
{
});


Task(Test)
    .IsDependentOn(Compile)
    .Does(() =>
{
    EnsureDirectoryExists(testResultsPath);

    var projects = GetTestProjectFiles(TestSrc);
    foreach(var project in projects)
    {
        DotNetCoreTest(
            project.FullPath,
            new DotNetCoreTestSettings()
            {
                Configuration = configuration,                
                ResultsDirectory = DirectoryPath.FromString(testResultsPath)
            });
    }    
});


Task(Analyze)
    .IsDependentOn(Build)
    .Does(() =>
{
    Information(Analyze);
});


Task(Pack)
    .IsDependentOn(Test)
    .IsDependentOn(Analyze)
    .Does(() =>
{
    var packSettings = new DotNetCorePackSettings 
    { 
        NoBuild = true
    };

    EnsureDirectoryExists(artifactsPath);
});


Task(Publish)
    .IsDependentOn(Pack)
    .Does(() =>
{
    Information(Publish);
});


RunTarget(target);
