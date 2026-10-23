# Ending-screen validation: one rules-driven playthrough per ending (no -ending override), plus a NEW RUN path.
param([string] $Exe = "D:\game dev\Builds\Game2D\TheLastAssessment.exe", [string] $Out = "D:\game dev\Builds\Game2D_logs\endscreens")
New-Item -ItemType Directory -Force $Out | Out-Null
$data = Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\TheLastAssessment"
$save2d = Join-Path $data "assessment2d_save.json"; $save3d = Join-Path $data "assessment_save.json"
$h3 = (Get-FileHash $save3d).Hash
$runs = @(
  @("E1", "q16=C,q18=B,q12=A,q20=A", ""), @("E2", "q16=A,q18=A,q20=C", ""), @("E3", "q9=B,q18=A,q20=D", ""),
  @("E4", "q16=C,q18=A,q20=C", ""),       @("E5", "q12=A,q20=B", ""),       @("E6", "q18=B,q12=A,q20=C", ""),
  @("NEWRUN_E5", "q12=A,q20=B", "-newrun")
)
foreach ($r in $runs) {
  if (Test-Path $save2d) { Remove-Item $save2d }
  $log = Join-Path $Out "$($r[0]).log"
  $p = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width 960 -screen-height 540 -autoplay -default A -answers $($r[1]) $($r[2]) -logFile `"$log`"" -PassThru
  if (-not $p.WaitForExit(300000)) { Stop-Process -Id $p.Id; "$($r[0]) TIMEOUT" }
  $ending = (Select-String $log -Pattern "\[Slice2D\] ENDING (\S+)").Matches | ForEach-Object { $_.Groups[1].Value }
  $screens = (Select-String $log -Pattern "\[Slice2D\] ENDSCREEN").Count
  $save = if (Test-Path $save2d) { (Get-Content $save2d -Raw | ConvertFrom-Json).currentNode } else { "(none)" }
  $newrun = (Select-String $log -Pattern "\[Slice2D\] (NEW RUN|START from NEW RUN)").Count
  "{0}: ending={1} endscreens={2} newRunEvents={3} 2Dsave.currentNode={4} 3DsaveUnchanged={5}" -f $r[0], ($ending -join ","), $screens, $newrun, $save, ((Get-FileHash $save3d).Hash -eq $h3)
}
