# Launches the 2D slice build windowed with an optional planted 2D save, waits, screenshots the
# window, then closes it. Prints the [Slice2D] trace lines from the player log.
# Usage: launch_slice.ps1 -Name r1 -Width 1280 -Height 720 [-Save '<json>' | -NoSave] [-Seconds 7] [-Keep]
param(
    [Parameter(Mandatory)] [string] $Name,
    [int] $Width = 1280, [int] $Height = 720,
    [string] $Save, [switch] $NoSave,
    [int] $Seconds = 7,
    [switch] $Keep,
    [string] $Exe = "D:\game dev\Builds\Slice2D\TheLastAssessment.exe",
    [string] $OutDir = "D:\game dev\Builds\Slice2D_logs\runs"
)
$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force $OutDir | Out-Null
$data = Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\TheLastAssessment"
$save2d = Join-Path $data "assessment2d_save.json"
if ($NoSave) { if (Test-Path $save2d) { Remove-Item $save2d } }
elseif ($PSBoundParameters.ContainsKey('Save')) { [IO.File]::WriteAllText($save2d, $Save) }

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class Win { [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h, ref POINT p);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  public struct RECT { public int L, T, R, B; } public struct POINT { public int X, Y; } }
"@

$log = Join-Path $OutDir "$Name.log"
$p = Start-Process $Exe -ArgumentList "-screen-fullscreen 0 -screen-width $Width -screen-height $Height -logFile `"$log`"" -PassThru
$c = New-Object Win+RECT
for ($i = 0; $i -lt 40; $i++) {  # the first launch can take a while to create its window
    Start-Sleep -Milliseconds 500; $p.Refresh(); $h = $p.MainWindowHandle
    if ($h -ne 0 -and [Win]::GetClientRect($h, [ref]$c) -and $c.R -gt 0) { break }
}
Start-Sleep -Seconds $Seconds
[Win]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 400
[Win]::GetClientRect($h, [ref]$c) | Out-Null
$o = New-Object Win+POINT; [Win]::ClientToScreen($h, [ref]$o) | Out-Null
$w = $c.R - $c.L; $hgt = $c.B - $c.T
$bmp = New-Object System.Drawing.Bitmap $w, $hgt
$g = [System.Drawing.Graphics]::FromImage($bmp)
# PrintWindow with PW_CLIENTONLY|PW_RENDERFULLCONTENT captures the game even when other windows overlap it.
$hdc = $g.GetHdc(); [Win]::PrintWindow($h, $hdc, 3) | Out-Null; $g.ReleaseHdc($hdc)
$png = Join-Path $OutDir "$Name.png"; $bmp.Save($png, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"client ${w}x${hgt} at $($o.X),$($o.Y) -> $png"
if (-not $Keep) { Stop-Process -Id $p.Id; Start-Sleep -Milliseconds 800 }
Select-String -Path $log -Pattern "\[Slice2D\]|could not read save|Exception" | ForEach-Object { $_.Line }
if (Test-Path $save2d) { "2D save now: " + ((Get-Content $save2d -Raw) -replace '\s+', ' ') } else { "2D save now: (none)" }
