int GetBuildNumber()
{
    const string paramName = "BuildNumber";
    return
        HasArgument(paramName) ? Argument<int>(paramName) :
        AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
        TravisCI.IsRunningOnTravisCI ? TravisCI.Environment.Build.BuildNumber :
        0;
}

void GetSemVerAndGitInfo(out string semVer, out string branch, out string commit)
{
    const string paramName = "Version";
    var v = GitVersion(new GitVersionSettings { RepositoryPath = "." });
    semVer = HasArgument(paramName) ? Argument<string>(paramName) : v.SemVer;
    branch = v.BranchName;
    commit = v.Sha;
}

DirectoryPath GetAbsDirPathFromArg(string argName, string defaultValue)
{
	return DirectoryPath.FromString(Argument(argName, defaultValue)).MakeAbsolute(Context.Environment);
}

FilePath GetSlnFile(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(normRoot + "/*.sln").First();
}

FilePathCollection GetProjectFiles(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(normRoot + "/**/*.csproj");
}

FilePathCollection GetTestProjectFiles(string root)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(normRoot + "/**/*.Tests.csproj");
}

FilePathCollection GetTestProjectDlls(string root, string configuration)
{
    var normRoot = DirectoryPath.FromString(root).MakeAbsolute(Context.Environment).FullPath;
	return GetFiles(string.Format("{0}/**/bin/{1}/**/*.Tests.dll", normRoot, configuration));
}

string GetLatestInstalledRuntimeVersion()
{
    var dotnetInfo = new System.Diagnostics.Process
    {
        StartInfo =
        {
            FileName = "dotnet",
            Arguments = "--info",
            UseShellExecute = false,
            CreateNoWindow = false,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true
        }
    };
    var output = string.Empty;
    using (dotnetInfo)
    {
        dotnetInfo.Start();
        output = dotnetInfo.StandardOutput.ReadToEnd();
        dotnetInfo.WaitForExit();
    }

    const string host = ".NET Core Shared Framework Host";
    var idx = output.IndexOf(host);
    if (idx > 0)
    {
        var rx = new System.Text.RegularExpressions.Regex(@"Version\s*:\s*(\d\.\d\.\d)");
        var m = rx.Match(output, idx + host.Length);
        if (m.Success)
        {
            return m.Groups[1].Value;
        }
    }

    return null;
}

string GetFxVersionArgForXunit()
{
    var runtime = GetLatestInstalledRuntimeVersion();
    return runtime == null
        ? string.Empty
        : "-fxversion " + runtime;
}
