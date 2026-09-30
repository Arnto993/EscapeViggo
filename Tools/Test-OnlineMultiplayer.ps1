param([switch]$Graphics, [switch]$EndDayStress, [switch]$Recovery, [switch]$KillHost)
$ErrorActionPreference = 'Stop'
if ($KillHost -and !$Recovery) { throw '-KillHost requires -Recovery.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$playerPath = Join-Path $projectRoot 'Builds/Windows/EscapeViggo.exe'
if (!(Test-Path -LiteralPath $playerPath)) { throw 'Build the Windows development player first.' }
$runDirectory = Join-Path $projectRoot ('Logs/Online-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$codeFile = Join-Path $runDirectory 'join-code.txt'
$processes = @()
try {
    foreach ($mode in @('host', 'client')) {
        $arguments = @('-ev-smoke', '-ev-online', "-ev-$mode", '-ev-online-profile', "m17_$mode",
            '-ev-online-sync-dir', ('"{0}"' -f $runDirectory),
            '-logFile', ('"{0}"' -f (Join-Path $runDirectory "$mode.log")))
        if ($mode -eq 'host') { $arguments += @('-ev-online-code-file', ('"{0}"' -f $codeFile)) }
        else { $arguments += @('-ev-join-code', (Get-Content -LiteralPath $codeFile -Raw).Trim()) }
        if ($EndDayStress) { $arguments += '-ev-endday-stress' }
        if ($Recovery) { $arguments = @($arguments | Where-Object { $_ -ne '-ev-smoke' }); $arguments += '-ev-online-recovery' }
        if ($KillHost) { $arguments += '-ev-kill-host' }
        if ($Graphics) { $arguments += @('-screen-width', '1280', '-screen-height', '720') }
        else { $arguments += @('-batchmode', '-nographics') }
        $process = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $null = $process.Handle
        $processes += $process
        if ($mode -eq 'host') {
            $deadline = (Get-Date).AddSeconds(90)
            while (!(Test-Path -LiteralPath $codeFile)) {
                if ($process.HasExited -or (Get-Date) -gt $deadline) { throw "Online host did not create a session. See $runDirectory/host.log" }
                $hostText = Get-Content (Join-Path $runDirectory 'host.log') -Raw -ErrorAction SilentlyContinue
                if ($hostText -match 'EV_ONLINE (?!Connected online)') { throw "Online service setup failed. See $runDirectory/host.log" }
                Start-Sleep -Milliseconds 500
            }
        }
    }
    $deadline = (Get-Date).AddSeconds(240)
    while (@($processes | Where-Object { !$_.HasExited }).Count -gt 0) {
        if ((Get-Date) -gt $deadline) { throw "Online gameplay test timed out. Logs: $runDirectory" }
        if ($KillHost -and !$processes[0].HasExited) {
            $clientReady = (Get-Content (Join-Path $runDirectory 'client.log') -Raw -ErrorAction SilentlyContinue) -match 'EV_ONLINE_RECOVERY READY'
            $hostReady = (Get-Content (Join-Path $runDirectory 'host.log') -Raw) -match 'EV_ONLINE_RECOVERY READY'
            if ($clientReady -and $hostReady) { Stop-Process -Id $processes[0].Id -Force }
        }
        Start-Sleep -Milliseconds 500
    }
    foreach ($process in $processes) {
        if ($KillHost -and $process.Id -eq $processes[0].Id) { continue }
        if ($process.ExitCode -ne 0) { throw "Online player failed. Logs: $runDirectory" }
    }
    $hostLog = Get-Content (Join-Path $runDirectory 'host.log')
    $clientLog = Get-Content (Join-Path $runDirectory 'client.log')
    if ($Recovery) {
        if (!($clientLog -match 'EV_ONLINE_RECOVERY PASSED CLIENT')) { throw "Recovery failed. Logs: $runDirectory" }
        Write-Output "PASS: real Relay host loss (force-killed: $KillHost), cleanup, online rehost, direct fallback. Logs: $runDirectory"
        return
    }
    foreach ($marker in @('EV_ONLINE Connected online', 'EV_INTERACTION PASSED', 'EV_TASK PASSED',
        'EV_STAMINA PASSED', 'EV_REST PASSED', 'EV_BATTERY PASSED', 'EV_RAGE PASSED', 'EV_CAPTURE PASSED',
        'EV_RESULTS PASSED', 'EV_WORKDAY END PASSED', 'EV_SMOKE PASSED')) {
        if (!($hostLog -match $marker) -or !($clientLog -match $marker)) { throw "Missing online check $marker. Logs: $runDirectory" }
    }
    $hostResults = @($hostLog | Where-Object { $_ -match '^EV_RESULTS_SNAPSHOT ' })
    $clientResults = @($clientLog | Where-Object { $_ -match '^EV_RESULTS_SNAPSHOT ' })
    if ($hostResults.Count -ne 1 -or $clientResults.Count -ne 1 -or $hostResults[0] -ne $clientResults[0]) { throw 'Online results disagree.' }
    Write-Output "PASS: real UGS Sessions / DTLS Relay, two processes on one PC, full workday, reconnect and host restart. Separate networks still require human testing. Logs: $runDirectory"
} finally {
    foreach ($process in $processes) { if (!$process.HasExited) { Stop-Process -Id $process.Id -Force } }
}
