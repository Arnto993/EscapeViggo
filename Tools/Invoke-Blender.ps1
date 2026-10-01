param(
    [string]$BlenderPath,
    [string]$Script,
    [string[]]$ScriptArguments = @(),
    [switch]$Version
)
$ErrorActionPreference = 'Stop'
try {
    if (!$BlenderPath) { $BlenderPath = $env:BLENDER_EXE }
    if (!$BlenderPath) {
        $command = Get-Command blender.exe -ErrorAction SilentlyContinue
        if ($command) { $BlenderPath = $command.Source }
    }
    if (!$BlenderPath) {
        $BlenderPath = Join-Path $env:ProgramFiles 'Blender Foundation/Blender 5.2/blender.exe'
    }
    if (!(Test-Path -LiteralPath $BlenderPath -PathType Leaf)) {
        throw 'Blender not found. Install Blender 5.2 LTS, pass -BlenderPath, or set BLENDER_EXE to blender.exe.'
    }
    $versionOutput = & $BlenderPath --version
    $versionExit = $LASTEXITCODE
    if ($versionExit -ne 0) { exit $versionExit }
    if (($versionOutput -join "`n") -notmatch '(?m)^Blender 5\.2\.\d+ LTS') {
        throw "Expected Blender 5.2 LTS; detected: $($versionOutput[0])"
    }
    Write-Host "$($versionOutput[0]) at $BlenderPath"
    if ($Version) { exit 0 }
    if (!$Script -or !(Test-Path -LiteralPath $Script -PathType Leaf)) {
        throw 'Provide an existing Python file with -Script (or use -Version for the smoke test).'
    }
    $scriptPath = (Resolve-Path -LiteralPath $Script).Path
    # Keep background-run preferences/extension caches out of the user's GUI profile.
    $env:BLENDER_USER_RESOURCES = Join-Path (Split-Path $PSScriptRoot -Parent) 'Logs/BlenderUser'
    New-Item -ItemType Directory -Force -Path $env:BLENDER_USER_RESOURCES | Out-Null
    & $BlenderPath --background --factory-startup --python-exit-code 1 --python $scriptPath -- @ScriptArguments
    $blenderExit = $LASTEXITCODE
    if ($blenderExit -ne 0) { Write-Warning "Blender failed with exit code $blenderExit. See Python/Blender diagnostics above." }
    exit $blenderExit
} catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
