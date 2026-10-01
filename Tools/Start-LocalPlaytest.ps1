param(
    [ValidateRange(2, 8)][int]$Players = 3,
    [ValidateSet(2, 5)][int]$Minutes = 5,
    [ValidateRange(1, 65535)][int]$Port = 7777,
    [switch]$DebugWeek
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$playerPath = Join-Path $projectRoot 'Builds/Windows/EscapeViggo.exe'
if (!(Test-Path -LiteralPath $playerPath)) { throw 'Build via Escape Viggo > Build > Windows Development Player first.' }
$runDirectory = Join-Path $projectRoot ('Logs/HumanPlaytest-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$dayMinutes = if ($DebugWeek) { '0.5' } else { $Minutes.ToString() }
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
# These are the interactive game windows requested by the person running this launcher.
for ($index = 0; $index -lt $Players; $index++) {
    $mode = if ($index -eq 0) { '-ev-host' } else { '-ev-client' }
    $arguments = @($mode, '-ev-address', '127.0.0.1', '-ev-port', $Port,
        '-ev-day-minutes', $dayMinutes, '-screen-fullscreen', '0',
        '-screen-width', '1280', '-screen-height', '720',
        '-logFile', ('"{0}"' -f (Join-Path $runDirectory "player-$index.log")))
    Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Normal | Out-Null
    if ($index -eq 0) { Start-Sleep -Seconds 2 }
}
Write-Output "Started $Players interactive windows for $dayMinutes-minute days. Host is Viggo on Monday; roles rotate daily. Esc opens the menu. Logs: $runDirectory"
