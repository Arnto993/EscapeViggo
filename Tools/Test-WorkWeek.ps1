param([int]$Port = 24777, [int]$Players = 3, [switch]$Online)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$playerPath = Join-Path $projectRoot 'Builds/Windows/EscapeViggo.exe'
$runDirectory = Join-Path $projectRoot ('Logs/Week-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + $Port)
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$codeFile = Join-Path $runDirectory 'code.txt'
$processes = @()
try {
    for ($i = 0; $i -lt $Players; $i++) {
        $mode = if ($i -eq 0) { '-ev-host' } else { '-ev-client' }
        $arguments = @('-batchmode', '-nographics', '-ev-week-smoke', $mode, '-ev-port', $Port,
            '-ev-day-minutes', '0.75', '-ev-week-peers', $Players, '-ev-week-sync-dir', ('"{0}"' -f $runDirectory),
            '-logFile', ('"{0}"' -f (Join-Path $runDirectory "player$i.log")))
        if ($Online) {
            $arguments += @('-ev-online', '-ev-online-profile', "week_$i")
            if ($i -eq 0) { $arguments += @('-ev-online-code-file', ('"{0}"' -f $codeFile)) }
            else { $arguments += @('-ev-join-code', (Get-Content -LiteralPath $codeFile -Raw).Trim()) }
        }
        $process = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
        $null = $process.Handle
        $processes += $process
        if ($i -eq 0 -and $Online) {
            $deadline = (Get-Date).AddSeconds(100)
            while (!(Test-Path -LiteralPath $codeFile)) {
                if ($process.HasExited -or (Get-Date) -gt $deadline) { throw "Relay host failed. Logs: $runDirectory" }
                Start-Sleep -Milliseconds 500
            }
        }
    }
    $deadline = (Get-Date).AddSeconds(380)
    while (@($processes | Where-Object { !$_.HasExited }).Count -gt 0) {
        if ((Get-Date) -gt $deadline) { throw "Week timed out. Logs: $runDirectory" }
        foreach ($process in $processes) { if ($process.HasExited -and $process.ExitCode -ne 0) { throw "Week player failed. Logs: $runDirectory" } }
        Start-Sleep -Milliseconds 500
    }
    $reference = @()
    for ($i = 0; $i -lt $Players; $i++) {
        $log = Get-Content (Join-Path $runDirectory "player$i.log")
        if (!($log -match '^EV_WEEK PASSED')) { throw "Missing success for player $i. Logs: $runDirectory" }
        $days = @($log | Where-Object { $_ -match '^EV_WEEK_DAY ' })
        if ($days.Count -ne 5) { throw 'Expected five daily snapshots.' }
        if ($i -eq 0) { $reference = $days }
        elseif (Compare-Object $reference $days) { throw 'Daily/weekly results disagree between peers.' }
    }
    Write-Output "PASS: five-day week, $Players processes, Relay=$Online, matching daily/weekly results. Logs: $runDirectory"
} finally {
    foreach ($process in $processes) { if (!$process.HasExited) { Stop-Process -Id $process.Id -Force } }
}
