param(
    [string]$Method = 'EscapeViggo.Editor.WorksiteValidation.Validate',
    [string]$LogName = 'Validation',
    [string]$TestPlatform
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$editorVersion = ((Get-Content (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt'))[0] -split ': ')[1]
$editorPath = "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
$logDirectory = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$unityArguments = @('-batchmode', '-nographics', '-projectPath', ('"{0}"' -f $projectRoot),
    '-logFile', ('"{0}"' -f (Join-Path $logDirectory "$LogName.log")))
if ($TestPlatform) {
    $unityArguments += @('-runTests', '-testPlatform', $TestPlatform,
        '-testResults', ('"{0}"' -f (Join-Path $logDirectory "$LogName.xml")))
} else {
    $unityArguments += @('-quit', '-executeMethod', $Method)
}
$unityRun = Start-Process -FilePath $editorPath -ArgumentList $unityArguments -WindowStyle Hidden -PassThru
$null = $unityRun.Handle
$unityRun.WaitForExit()
Write-Output "Unity exit: $($unityRun.ExitCode); log: Logs/$LogName.log"
if ($unityRun.ExitCode -ne 0) {
    Get-Content (Join-Path $logDirectory "$LogName.log") -Tail 45
    throw "Unity validation failed."
}
