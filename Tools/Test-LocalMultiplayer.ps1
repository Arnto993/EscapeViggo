param([int]$Port = 17777, [switch]$Graphics)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$playerPath = Join-Path $projectRoot 'Builds/Windows/EscapeViggo.exe'
if (!(Test-Path -LiteralPath $playerPath)) { throw 'Build the Windows development player first.' }
$runDirectory = Join-Path $projectRoot ('Logs/Multiplayer-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$processes = @()
try {
    foreach ($mode in @('host', 'client')) {
        $logPath = Join-Path $runDirectory "$mode.log"
        $arguments = @('-ev-smoke', "-ev-$mode", '-ev-port', $Port,
            '-logFile', ('"{0}"' -f $logPath))
        if ($Graphics) {
            $arguments += @('-screen-width', '960', '-screen-height', '540',
                '-ev-capture-dir', ('"{0}"' -f $runDirectory))
        } else { $arguments += @('-batchmode', '-nographics') }
        $process = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $null = $process.Handle
        $processes += $process
        if ($mode -eq 'host') { Start-Sleep -Seconds 2 }
    }
    $deadline = (Get-Date).AddSeconds(110)
    while (@($processes | Where-Object { !$_.HasExited }).Count -gt 0) {
        if ((Get-Date) -gt $deadline) { throw 'Multiplayer smoke test timed out.' }
        Start-Sleep -Milliseconds 500
    }
    foreach ($process in $processes) {
        if ($process.ExitCode -ne 0) { throw "Player failed with exit $($process.ExitCode). Logs: $runDirectory" }
    }
    $hostLog = Get-Content (Join-Path $runDirectory 'host.log')
    $clientLog = Get-Content (Join-Path $runDirectory 'client.log')
    if (!($hostLog -match 'EV_SMOKE PASSED HOST') -or !($clientLog -match 'EV_SMOKE PASSED CLIENT')) {
        throw 'Missing success markers in player logs.'
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
    Write-Output "PASS: two processes, independent movement, ownership, cameras/audio, crouch, four matching poses, reconnect and host restart. Logs: $runDirectory"
} finally {
    foreach ($process in $processes) {
        if (!$process.HasExited) { Stop-Process -Id $process.Id -Force }
    }
}
