# Runs the reviewed ending paths (EndingRules_FinalReview.md §5) on the Windows build with the live,
# approved rules (no -ending override) and records the selected ending plus the final save state.
param([string] $Exe = "D:\game dev\Builds\Game2D\TheLastAssessment.exe",
      [string] $Out = "D:\game dev\Builds\Game2D_logs\paths")
New-Item -ItemType Directory -Force $Out | Out-Null
$save = Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\TheLastAssessment\assessment2d_save.json"
$paths = @(
  @("p01_E6_hit_C",          "q18=B,q12=A,q20=C",            "ending6"),
  @("p02_E6_hit_D_keptC",    "q18=B,q12=C,q20=D",            "ending6"),
  @("p03_E6_miss_skipped",   "q18=B,q12=D,q16=C,q20=C",      "ending4"),
  @("p04_E6_miss_q18",       "q18=C,q12=A,q20=C",            "ending2"),
  @("p05_E5_hit",            "q12=A,q20=B",                  "ending5"),
  @("p06_E5_hit_keptC",      "q12=C,q20=B",                  "ending5"),
  @("p07_E5_miss_skipped",   "q12=D,q20=B",                  "ending2"),
  @("p08_E4_hit",            "q16=C,q18=A,q20=C",            "ending4"),
  @("p09_E4_miss_q16",       "q16=B,q18=A,q20=C",            "ending2"),
  @("p10_E3_hit_q9B",        "q9=B,q18=A,q20=D",             "ending3"),
  @("p11_E3_hit_q17B",       "q9=A,q17=B,q18=A,q20=D",       "ending3"),
  @("p12_E3_miss",           "q9=A,q17=A,q18=A,q20=D",       "ending2"),
  @("p13_E3_shadowed_E6",    "q9=B,q18=B,q12=A,q20=D",       "ending6"),
  @("p14_E1_hit",            "q16=C,q18=B,q12=A,q20=A",      "ending1"),
  @("p15_E2_via_B",          "q12=D,q20=B",                  "ending2"),
  @("p16_E2_via_C",          "q16=A,q18=A,q20=C",            "ending2"),
  @("p17_Q12B_E5",           "q12=B,q20=B",                  "ending5")
)
foreach ($p in $paths) {
  if (Test-Path $save) { Remove-Item $save }
  $log = Join-Path $Out "$($p[0]).log"
  $proc = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width 960 -screen-height 540 -autoplay -default A -answers $($p[1]) -logFile `"$log`"" -PassThru
  if (-not $proc.WaitForExit(300000)) { Stop-Process -Id $proc.Id; "$($p[0]) TIMEOUT" }
  $got = (Select-String $log -Pattern "\[Slice2D\] ENDING (\S+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ","
  $s = Get-Content $save -Raw | ConvertFrom-Json
  $heard = $s.flags -contains "residual_audio_heard"; $kept = $s.evidence -contains "q12_recording"
  "{0}|{1}|expected={2}|got={3}|{4}|heard={5}|kept={6}" -f $p[0], $p[1], $p[2], $got, ($(if ($got -eq $p[2]) { "PASS" } else { "FAIL" })), $heard, $kept
}
# Save/resume: stop mid-game at Q12, then resume and answer C there.
[IO.File]::WriteAllText($save, '{"currentNode":"q12","answers":[{"node":"q1","choice":"A"}],"flags":[],"evidence":[]}')
$log = Join-Path $Out "p18_resume_q12C.log"
$proc = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width 960 -screen-height 540 -autoplay -resume -default A -answers q12=C,q20=B -logFile `"$log`"" -PassThru
[void]$proc.WaitForExit(300000)
$got = (Select-String $log -Pattern "\[Slice2D\] ENDING (\S+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ","
$start = (Select-String $log -Pattern "\[Slice2D\] START .*" | ForEach-Object { $_.Matches[0].Value }) -join ""
$s = Get-Content $save -Raw | ConvertFrom-Json
"p18_resume_q12C|{0}|expected=ending5|got={1}|{2}|heard={3}|kept={4}" -f $start, $got, ($(if ($got -eq "ending5") { "PASS" } else { "FAIL" })), ($s.flags -contains "residual_audio_heard"), ($s.evidence -contains "q12_recording")
