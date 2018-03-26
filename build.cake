#load "utils.cake"
#tool "nuget:?package=GitVersion.CommandLine"

const string Clean = "Clean";
const string Restore = "Restore";
const string Compile = "Compile";
const string ReBuild = "ReBuild";
const string Test = "Test";
const string Pack = "Pack";

const string Src = "./src";
const string TestSrc = "./test";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("Target", Pack);
var configuration = Argument("Configuration", "Release");
var verbosity = Argument<DotNetCoreVerbosity>("Verbosity", DotNetCoreVerbosity.Minimal);
var artifactsPath = DirectoryPath.FromString(Argument("Artifacts", "./artifacts")).MakeAbsolute(Context.Environment);
var testResultsPath = DirectoryPath.FromString(Argument("TestResults", "./test-results")).MakeAbsolute(Context.Environment);


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = GetSlnFile(".").FullPath;
var buildNumber = GetBuildNumber();
string semVer;
string branch;
string commit;
GetSemVerAndGitInfo(out semVer, out branch, out commit);
Information("version: {0}, build: {1}, configuration: {2}, commit: {3}, branch: {4})", semVer, buildNumber, configuration, commit, branch);


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task(Clean)
    .Description("Deletes contents of build directories (bin, obj, artifacts and test results).")
    .Does(() =>
{
    var settings = new DeleteDirectorySettings { Recursive = true, Force = true };
    if (DirectoryExists(artifactsPath)) DeleteDirectory(artifactsPath, settings);
    if (DirectoryExists(testResultsPath)) DeleteDirectory(testResultsPath, settings);
    DeleteDirectories(GetDirectories("./**/bin"), settings);
    DeleteDirectories(GetDirectories("./**/obj"), settings);
});


Task(Restore)
    .Description("Restores NuGet packages.")
    .Does(() =>
{
    DotNetCoreRestore(solutionPath);
});


Task(Compile)
    .Description("Compiles all projects linked by solution file.")
    .Does(() =>
{
    var buildSettings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
        Verbosity = verbosity,
        MSBuildSettings = new DotNetCoreMSBuildSettings
        {
            ArgumentCustomization = args => args
                                    .Append("-p:SemVer=" + semVer)
                                    .Append("-p:BuildNumber=" + buildNumber)
                                    .Append("-p:GitCommit" + commit)
                                    .Append("-p:GitBranch" + branch),
            NoLogo = true,
            Verbosity = verbosity,
            DetailedSummary = false,
        }
    };
    DotNetCoreBuild(solutionPath, buildSettings);
});


Task(ReBuild)
    .Description("Combines tasks Clean, Restore and Compile (in that order).")
    .IsDependentOn(Clean)
    .IsDependentOn(Restore)
    .IsDependentOn(Compile)
    .Does(() =>
{
});


Task(Test)
    .Description("Executes all tests; runs first: Compile.")
    .IsDependentOn(Compile)
    .Does(() =>
{
    EnsureDirectoryExists(testResultsPath);
    var fxversion = GetFxVersionArgForXunit();
    var projects = GetTestProjectFiles(TestSrc);
    foreach(var project in projects)
    {
        var xml = testResultsPath.CombineWithFilePath(project.GetFilenameWithoutExtension()).FullPath + ".xml";
        var args = string.Format("-configuration {0} -nobuild -nologo {1} -xml {2}", configuration, fxversion, xml);
        DotNetCoreTool(
            projectPath: project.FullPath,
            command: "xunit",
            arguments: args
        );
    }
});


Task(Pack)
    .Description("Packages NuGet packages; runs first: Clean, Restore, Compile, Test.")
    .IsDependentOn(Clean)
    .IsDependentOn(Restore)
    .IsDependentOn(Compile)
    .IsDependentOn(Test)
    .Does(() =>
{
    EnsureDirectoryExists(artifactsPath);

    var revision = buildNumber.ToString("D4");
    var projects = GetProjectFiles(Src);
    foreach (var project in projects)
    {
        DotNetCorePack(
            project.FullPath,
            new DotNetCorePackSettings()
            {
                Configuration = configuration,
                OutputDirectory = artifactsPath,
                NoRestore = true,
                NoBuild = true,
                Verbosity = verbosity,
            });
    }
});


RunTarget(target);
