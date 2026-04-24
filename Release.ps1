[CmdletBinding()]
param(
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release',
    [string]$OutputDirectory = 'release',
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $repoRoot 'DriveIcons.App\DriveIcons.App.csproj'
$publishDirectory = Join-Path $repoRoot 'publish\driveicons'
$artifactDirectory =
    if ([System.IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $repoRoot $OutputDirectory }

Push-Location $repoRoot
try {
    [xml]$project = Get-Content -Path $projectPath
    $version = $project.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "App version was not found in '$projectPath'."
    }

    $artifactName = "driveicons-$version.exe"
    $artifactPath = Join-Path $artifactDirectory $artifactName

    if (Test-Path $publishDirectory) {
        Remove-Item $publishDirectory -Recurse -Force
    }

    dotnet restore DriveIcons.sln
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet restore failed.'
    }

    dotnet build DriveIcons.sln --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet build failed.'
    }

    if (-not $SkipTests) {
        dotnet test DriveIcons.sln --configuration $Configuration --no-build --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw 'dotnet test failed.'
        }
    }

    dotnet publish DriveIcons.App\DriveIcons.App.csproj `
        --configuration $Configuration `
        --runtime $Runtime `
        --self-contained true `
        /p:PublishSingleFile=true `
        /p:PublishTrimmed=false `
        /p:IncludeNativeLibrariesForSelfExtract=true `
        /p:DebugType=None `
        /p:DebugSymbols=false `
        --output $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet publish failed.'
    }

    $publishedExe = Join-Path $publishDirectory 'DriveIcons.App.exe'
    if (-not (Test-Path $publishedExe)) {
        throw "Published executable was not found at '$publishedExe'."
    }

    New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
    Copy-Item $publishedExe $artifactPath -Force

    Write-Host "Created release artifact: $artifactPath"
}
finally {
    Pop-Location
}
