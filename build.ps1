param (
    [ValidatePattern('^\d+(\.\d+){2}(\-rc\d*)?$')][string] $Version = '1.0.0',
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Release',
    [string] $ForceNugetPackagesRoot,    
    [switch] $SkipCodeAnalysis,
    [switch] $WarningsAsErrors,
    [switch] $Verbose,
    [switch] $PublishToNuGet,
    [string] $NuGetApiKey
)

. shared-solution-files\functions.ps1

Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$SlnFile = 'eventr.sln'
$CodeAnalysis = if ($SkipCodeAnalysis) {"False"} else {"True"}
$Warnings = if ($WarningsAsErrors) {"True"} else {"False"}
$Verbosity = if ($Verbose) {"normal"} else {"minimal"}
if ($ForceNugetPackagesRoot -and ($ForceNugetPackagesRoot -ne '')) {
    $NugetPackagesRoot = $ForceNugetPackagesRoot
} else {
    $NugetPackagesRoot = $global:NUGET_PACKAGES_ROOT
}

# clean up
Clear-SolutionDirectory

# display build info
Write-BuildInfo -NugetPackagesRoot $NugetPackagesRoot

# build
Write-Label "Building $SlnFile"
& $global:DOTNET_PATH build $SlnFile -c $Configuration -v $Verbosity /p:Version=$Version /p:RestorePackagesPath=$NugetPackagesRoot /p:CodeAnalysis=$CodeAnalysis /p:WarningsAsErrors=$Warnings /nologo

# create packages
Get-ChildItem '.\src' -Include '*.csproj' -Recurse | ForEach-Object {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
    Write-Label "Packaging $projName"
    & $global:DOTNET_PATH pack $_.FullName -c $Configuration -o $global:PUBLISH_DIR -v $Verbosity --no-build --no-restore /p:Version=$Version /nologo
}

# publish
if ($PublishToNuGet) {
    Publish-NuGetPackages
}

Pop-Location
