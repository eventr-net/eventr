int GetBuildNumber()
{
    const string paramName = "BuildNumber";
    return 
        HasArgument(paramName) ? Argument<int>(paramName) :
        AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
        TravisCI.IsRunningOnTravisCI ? TravisCI.Environment.Build.BuildNumber :
        EnvironmentVariable(paramName) != null ? int.Parse(EnvironmentVariable(paramName)) : 
        0;
}

string GetSemanticVersion()
{
    const string paramName = "Version";
    return
        HasArgument(paramName) ? Argument<string>(paramName) :
        "0.0.0";
}

FilePath GetSlnFile(string root)
{    
	return GetFiles(string.Format("{0}/*.sln", root)).First();
}

FilePathCollection GetProjectFiles(string root)
{
	return GetFiles(string.Format("{0}/**/*.csproj", root));
}

FilePathCollection GetTestProjectFiles(string root)
{
	return GetFiles(string.Format("{0}/**/*.Tests.csproj", root));
}

FilePathCollection GetTestProjectDlls(string root, string configuration)
{
	return GetFiles(string.Format("{0}/**/bin/{1}/**/*.Tests.dll", root, configuration));
}
