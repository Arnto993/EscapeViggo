param([int]$Port = 17777, [switch]$Graphics, [switch]$Observer, [switch]$EndDayStress, [switch]$ConnectionOnly)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$playerPath = Join-Path $projectRoot 'Builds/Windows/EscapeViggo.exe'
if (!(Test-Path -LiteralPath $playerPath)) { throw 'Build the Windows development player first.' }
$runDirectory = Join-Path $projectRoot ('Logs/Multiplayer-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + $Port)
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$processes = @()
try {
    foreach ($mode in @('host', 'client')) {
        $logPath = Join-Path $runDirectory "$mode.log"
        $arguments = @('-ev-smoke', "-ev-$mode", '-ev-port', $Port,
            '-logFile', ('"{0}"' -f $logPath))
        if ($ConnectionOnly) { $arguments = @('-ev-connection-smoke', "-ev-$mode", '-ev-port', $Port, '-logFile', ('"{0}"' -f $logPath)) }
        if ($EndDayStress) { $arguments += '-ev-endday-stress' }
        if ($Graphics) {
            $arguments += @('-screen-width', '960', '-screen-height', '540',
                '-ev-capture-dir', ('"{0}"' -f $runDirectory))
        } else { $arguments += @('-batchmode', '-nographics') }
        $process = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $null = $process.Handle
        $processes += $process
        if ($mode -eq 'host') { Start-Sleep -Seconds 2 }
    }
    $deadline = (Get-Date).AddSeconds(180)
    $observerStarted = $false
    while (@($processes | Where-Object { !$_.HasExited }).Count -gt 0) {
        if ((Get-Date) -gt $deadline) { throw 'Multiplayer smoke test timed out.' }
        if (!$ConnectionOnly -and $Observer -and !$observerStarted -and ((Get-Content (Join-Path $runDirectory 'host.log')) -match '^EV_TASK PASSED')) {
            $observerArguments = @('-batchmode', '-nographics', '-ev-observer', '-ev-client', '-ev-port', $Port,
                '-logFile', ('"{0}"' -f (Join-Path $runDirectory 'observer.log')))
            if ($EndDayStress) { $observerArguments += '-ev-endday-stress' }
            $observerProcess = Start-Process -FilePath $playerPath -ArgumentList $observerArguments -WindowStyle Hidden -PassThru
            $null = $observerProcess.Handle
            $processes += $observerProcess
            $observerStarted = $true
        }
        Start-Sleep -Milliseconds 500
    }
    foreach ($process in $processes) {
        if ($process.ExitCode -ne 0) { throw "Player failed with exit $($process.ExitCode). Logs: $runDirectory" }
    }
    $hostLog = Get-Content (Join-Path $runDirectory 'host.log')
    $clientLog = Get-Content (Join-Path $runDirectory 'client.log')
    if ($ConnectionOnly) {
        if (!($hostLog -match 'EV_CONNECTION PASSED HOST') -or !($clientLog -match 'EV_CONNECTION PASSED CLIENT')) {
            throw 'Connection lifecycle check failed.'
        }
        Write-Output "PASS: unreachable-host timeout, retry, configured workday/results, preset restart and cursor cleanup. Logs: $runDirectory"
        return
    }
    if (!($hostLog -match 'EV_SMOKE PASSED HOST') -or !($clientLog -match 'EV_SMOKE PASSED CLIENT')) {
        throw 'Missing success markers in player logs.'
    }
    foreach ($marker in @('EV_INTERACTION PASSED', 'EV_TASK PASSED', 'EV_SCORE owner=0 score=0', 'EV_STAMINA PASSED', 'EV_REST PASSED', 'EV_BATTERY PASSED', 'EV_RAGE PASSED', 'EV_CAPTURE PASSED', 'EV_RESULTS PASSED', 'EV_WORKDAY END PASSED')) {
        if (!($hostLog -match $marker) -or !($clientLog -match $marker)) { throw "Missing gameplay check: $marker" }
    }
    $hostScores = @($hostLog | Where-Object { $_ -match '^EV_SCORE ' } | Sort-Object)
    $clientScores = @($clientLog | Where-Object { $_ -match '^EV_SCORE ' } | Sort-Object)
    if ($hostScores.Count -lt 2 -or (Compare-Object $hostScores $clientScores)) { throw 'Host/client scores disagree.' }
    if ($Observer -and (!$observerStarted -or !((Get-Content (Join-Path $runDirectory 'observer.log')) -match 'EV_OBSERVER PASSED'))) {
        throw 'Late observer validation did not complete.'
    }
    $finalHost = @($hostLog | Where-Object { $_ -match '^EV_RESULTS_SNAPSHOT ' })
    $finalClient = @($clientLog | Where-Object { $_ -match '^EV_RESULTS_SNAPSHOT ' })
    if ($finalHost.Count -ne 1 -or $finalClient.Count -ne 1 -or $finalHost[0] -ne $finalClient[0]) { throw 'Final results disagree.' }
    if ($Observer) {
        $observerLog = Get-Content (Join-Path $runDirectory 'observer.log')
        if (!($observerLog -match 'EV_RESULTS_LATEJOIN PASSED') -or !($observerLog -contains $finalHost[0])) { throw 'Observer final results disagree.' }
    }
    if ($EndDayStress -and (!($hostLog -match 'EV_ENDDAY_ACTIVE_SETUP PASSED') -or !($clientLog -match 'EV_ENDDAY_ACTIVE_SETUP PASSED'))) {
        throw 'Active-task/Rage end-of-day setup was not exercised.'
    }
    $snapshots = @{}
    foreach ($line in $hostLog) {
        if ($line -match 'EV_SNAPSHOT (\{.*\})') {
            $snapshot = $Matches[1] | ConvertFrom-Json
            $snapshots["$($snapshot.round):$($snapshot.owner)"] = $snapshot
        }
    }
    $comparisons = 0
    foreach ($line in $clientLog) {
        if ($line -match 'EV_SNAPSHOT (\{.*\})') {
            $client = $Matches[1] | ConvertFrom-Json
            $server = $snapshots["$($client.round):$($client.owner)"]
            if (!$server) { throw 'Missing matching host snapshot.' }
            $distance = [Math]::Sqrt([Math]::Pow($server.x - $client.x, 2) + [Math]::Pow($server.y - $client.y, 2) + [Math]::Pow($server.z - $client.z, 2))
            if ($distance -gt 0.15 -or [Math]::Abs($server.yaw - $client.yaw) -gt 1 -or !$client.crouching -or !$server.crouching -or $server.role -ne $client.role) {
                throw "Host/client state mismatch: round $($client.round), player $($client.owner), distance $distance"
            }
            $comparisons++
        }
    }
    if ($comparisons -ne 4) { throw "Expected 4 matching snapshots, got $comparisons." }
    if ($Graphics -and (!(Test-Path (Join-Path $runDirectory 'host.png')) -or !(Test-Path (Join-Path $runDirectory 'client.png')))) {
        throw 'Rendered screenshots were not produced.'
    }
    Write-Output "PASS: movement, ownership, roles, clock, task/scoring, stamina/rest, theft/Rage, capture/respawn, frozen results, reconnect and host restart. Late observer: $observerStarted. Active cutoff: $EndDayStress. Logs: $runDirectory"
} finally {
    foreach ($process in $processes) {
        if (!$process.HasExited) { Stop-Process -Id $process.Id -Force }
    }
}
