param([Parameter(Mandatory=$true)][string]$RunDirectory, [switch]$Week)
$ErrorActionPreference = 'Stop'
# Run after Test-LocalMultiplayer/Test-OnlineMultiplayer or Test-WorkWeek.
# These are event-delivery assertions; headless logs cannot judge the audible mix.
$files = if ($Week) { Get-ChildItem -LiteralPath $RunDirectory -Filter 'player*.log' } else {
    @((Join-Path $RunDirectory 'host.log'), (Join-Path $RunDirectory 'client.log')) | Get-Item
}
foreach ($file in $files) {
    $lines = Get-Content -LiteralPath $file.FullName
    $counts = @{}
    foreach ($line in $lines) {
        if ($line -match '^EV_AUDIO cue=(\w+) object=') {
            $cue = $Matches[1]
            $counts[$cue] = 1 + [int]$counts[$cue]
        }
    }
    $expected = if ($Week) {
        @{ DayEnded = 5; WeekResults = 1; TaskCompleted = 20; PowerCut = 9; PowerRestored = 6; WorkInterrupted = 5;
            VoiceJavell = 2; Exhausted = 2; Capture = 2; BatteryStolen = 5; BatteryDiscovered = 5; BatteryRecovered = 2; RageStarted = 5;
            VoiceRage = 5; VoiceCapture = 2; VoiceBatteryRecovered = 2 }
    } else {
        @{ VoiceJavell = 1; Exhausted = 1; RestEnded = 1; BatteryStolen = 1; BatteryDiscovered = 1;
            BatteryRecovered = 1; RageStarted = 1; Capture = 1; TaskCompleted = 1; DayEnded = 1;
            VoiceRage = 1; VoiceCapture = 1; VoiceBatteryRecovered = 1 }
    }
    foreach ($cue in $expected.Keys) {
        if ([int]$counts[$cue] -ne $expected[$cue]) {
            throw "$($file.Name): expected $($expected[$cue]) $cue requests, got $([int]$counts[$cue])."
        }
    }
    if (!$Week) {
        foreach ($cue in @('TaskFailed', 'Tool', 'ElectricianStep', 'ViggoStep')) {
            if ([int]$counts[$cue] -lt 1) { throw "$($file.Name): $cue feedback was not exercised." }
        }
    }
}
Write-Output "PASS: M21 presentation event counts on $($files.Count) peers; Week=$Week. No duplicate checkpoint cues."
