# Screenshots every ending screen: resumes at Q20 with each path's earlier answers (live rules choose the ending).
param([string] $Exe = "D:\game dev\Builds\Game2D\TheLastAssessment.exe", [string] $Out = "D:\game dev\Builds\Game2D_logs\endshots_final")
New-Item -ItemType Directory -Force $Out | Out-Null
$save = Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\TheLastAssessment\assessment2d_save.json"
function Q20Save($ans) {
  $map = @{}; foreach ($i in 1..19) { $map["q$i"] = "A" }; foreach ($kv in ($ans -split ',' | Where-Object { $_ })) { $k, $v = $kv -split '='; $map[$k] = $v }
  $a = (1..19 | ForEach-Object { '{"node":"q' + $_ + '","choice":"' + $map["q$_"] + '"}' }) -join ','
  [IO.File]::WriteAllText($save, '{"currentNode":"q20","answers":[' + $a + '],"flags":["residual_audio_heard","evidence:q12_recording"],"evidence":["q12_recording"]}')
}
$runs = @(@("E1_169","q16=C,q18=B","A",1280,720),@("E2_169","q16=A,q18=A","C",1280,720),@("E3_169","q9=B,q18=A","D",1280,720),
          @("E4_169","q16=C,q18=A","C",1280,720),@("E5_169","","B",1280,720),@("E6_169","q18=B","C",1280,720),
          @("E5_219","","B",1600,686),@("E1_219","q16=C,q18=B","A",1600,686),@("E5_43","","B",1024,768),@("E1_43","q16=C,q18=B","A",1024,768))
foreach ($r in $runs) {
  Q20Save $r[1]; $dir = Join-Path $Out $r[0]; New-Item -ItemType Directory -Force $dir | Out-Null
  $p = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width $($r[3]) -screen-height $($r[4]) -autoplay -resume -answers q20=$($r[2]) -shots `"$dir`" -logFile `"$(Join-Path $Out "$($r[0]).log")`"" -PassThru
  if (-not $p.WaitForExit(180000)) { Stop-Process -Id $p.Id; "$($r[0]) TIMEOUT" }
  $log = Join-Path $Out "$($r[0]).log"
  "{0}: {1} bg={2} shots={3}" -f $r[0], ((Select-String $log -Pattern "\[Slice2D\] ENDING (\S+)").Matches.Groups[1].Value), (((Select-String $log -Pattern "ENDSCREEN_BG \S+ \d/\d (\S+)").Matches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique) -join ","), (Get-ChildItem $dir -Filter "*end_*").Count
}
