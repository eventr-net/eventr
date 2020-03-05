param (
    [ValidatePattern('^\d+(\.\d+){2}(\-rc\d+)?$')][string] $Version = '1.0.0',
    [ValidateSet('Debug', 'Release')][string] $Configuration = 'Release',
    [string] $ForceNugetPackagesRoot,
    [string] $NuGetApiKey,
    [switch] $SkipCodeAnalysis,
    [switch] $WarningsAsErrors,
    [switch] $Verbose,
    [switch] $PublishToNuGet
)

Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

function Get-NugetGlobalPackagesPath {
    param (
        [string] $NugetPath = 'nuget'
    )
    return (Invoke-Expression "$NugetPath locals global-packages -list" | Out-String).Replace("global-packages:", "").Trim()
}

# config
$SlnFile = 'eventr.sln'
$Dotnet = 'dotnet'
$Nuget = 'nuget'
$NugetFeedUrl = 'https://api.nuget.org/v3/index.json'
$OutputDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath('.\publish')
$CodeAnalysis = if ($SkipCodeAnalysis) {"False"} else {"True"}
$Warnings = if ($WarningsAsErrors) {"True"} else {"False"}
$Verbosity = if ($Verbose) {"normal"} else {"minimal"}
$NugetPackagesRoot = if ($ForceNugetPackagesRoot -ne '') {$ForceNugetPackagesRoot} else {Get-NugetGlobalPackagesPath -NugetPath $Nuget}

function Write-NugetInfo {
    param (
        [string] $NugetPath = 'nuget'
    )
    $NugetVersion = ((Invoke-Expression "$NugetPath help") | Select-Object -First 1 | Out-String).Replace("NuGet Version:", "").Trim()
    Write-Host "NuGet: $NugetPath ($NugetVersion)"
    Write-Host "NuGet locals:"
    (Invoke-Expression "$NugetPath locals all -list") | Write-Host -ForegroundColor DarkGray
}

function Write-DotnetInfo {
    param (
        [string] $DotnetPath = 'dotnet'
    )
    $DotnetVersion = ((Invoke-Expression "$DotnetPath --version") | Out-String).Trim()
    Write-Host "Dotnet CLI: $DotnetPath ($DotnetVersion)"
}

# build info
Write-Host '>>> Build info >>>' -ForegroundColor Yellow
$ParamList = (Get-Command -Name $MyInvocation.InvocationName).Parameters;
foreach ($key in $ParamList.keys) {
    $var = Get-Variable -Name $key -ErrorAction SilentlyContinue
    if ($var -and ($var.Value -ne '')) { Write-Host "-$($var.Name) $($var.Value)" }
}
Write-Host "Output: $OutputDir"
Write-Host "NugetPackagesRoot: $NugetPackagesRoot"
Write-Host ("Running as: " + [System.Security.Principal.WindowsIdentity]::GetCurrent().Name)
Write-DotnetInfo -DotnetPath $Dotnet
Write-NugetInfo -NugetPath $Nuget

# cleanup
Write-Host '>>> Cleaning solution >>>' -ForegroundColor Yellow
Get-ChildItem .\ -Include bin,obj,publish,packages -Recurse | ForEach-Object {
    Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue
}

# build projects
Write-Host ">>> Building $SlnFile >>>" -ForegroundColor Yellow
& $Dotnet build $SlnFile -c $Configuration -v $Verbosity /p:Version=$Version /p:RestorePackagesPath=$NugetPackagesRoot /p:CodeAnalysis=$CodeAnalysis /p:WarningsAsErrors=$Warnings /nologo
Get-ChildItem '.\src' -Include '*.csproj' -Recurse | ForEach-Object {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
    Write-Host ">>> Packaging $projName >>>" -ForegroundColor Yellow
    & $Dotnet pack $_.FullName -c $Configuration -o $OutputDir -v $Verbosity --no-build --no-restore /p:Version=$Version /nologo
}

# publish to NuGet
if ($PublishToNuGet) {
    $NuGetApiKey = if ($NuGetApiKey -ne '') {$NuGetApiKey} else {Get-Childitem env:NUGET_APIKEY_EVENTR}
    if (-not $NuGetApiKey) {
        Write-Error 'NuGet API key is not provided.'
        Exit 1
    }

    Get-ChildItem $OutputDir -Include '*.nupkg' | ForEach-Object {
        $nugetFilename = Split-Path $_.FullName -Leaf
        Write-Host ">>> Publishing $nugetFilename to NuGet >>>" -ForegroundColor Yellow
        & $Nuget push $_.FullName -k $NuGetApiKey -s $NugetFeedUrl
    }
}

Pop-Location