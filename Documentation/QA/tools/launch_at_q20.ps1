# Writes a 2D save positioned at Q20 with the given earlier answers (as a real playthrough would leave it),
# then launches the build normally (no autoplay, no forced ending) for real-input testing via Continue.
# Usage: launch_at_q20.ps1 -Name e6 -Answers "q18=B,q12=A" [-Kept] [-Heard]
param([string] $Name, [string] $Node = "q20", [string] $Answers = "", [switch] $Kept, [switch] $Heard,
      [string] $Exe = "D:\game dev\Builds\Game2D\TheLastAssessment.exe", [string] $Out = "D:\game dev\Builds\Game2D_logs\manual")
New-Item -ItemType Directory -Force $Out | Out-Null
$map = @{}; foreach ($i in 1..19) { $map["q$i"] = "A" }
foreach ($kv in ($Answers -split ',' | Where-Object { $_ })) { $k, $v = $kv -split '='; $map[$k] = $v }
$answers = (1..19 | ForEach-Object { '{"node":"q' + $_ + '","choice":"' + $map["q$_"] + '"}' }) -join ','
$flags = @(); if ($Heard) { $flags += '"residual_audio_heard"' }; if ($Kept) { $flags += '"evidence:q12_recording"' }
$evidence = if ($Kept) { '"q12_recording"' } else { '' }
$json = '{"currentNode":"' + $Node + '","answers":[' + $answers + '],"flags":[' + ($flags -join ',') + '],"evidence":[' + $evidence + ']}'
[IO.File]::WriteAllText((Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\TheLastAssessment\assessment2d_save.json"), $json)
Get-Process TheLastAssessment -ErrorAction SilentlyContinue | Stop-Process
$p = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile `"$(Join-Path $Out "$Name.log")`"" -PassThru
Start-Sleep 5; (New-Object -ComObject WScript.Shell).AppActivate([int]$p.Id) | Out-Null
"launched $Name"

