# ビルド後に実行
# - SylphyHorn.exe (とその関連ファイル) を除くすべてのファイルを lib フォルダーに移動

Param ( $TargetDir )

$targets = $TargetDir
$lib = ($TargetDir + "lib\")
$excludes = ".assets", "AppxManifest.xml", "SylphyHorn.exe*", "SylphyHorn.pdb", "SchedulerManager.exe*", "SchedulerManager.pdb"

if ( Test-Path $lib ) {
    Remove-Item $lib -Recurse
}

New-Item $lib -ItemType Directory

Get-ChildItem $targets -Exclude $excludes | Move-Item -Destination $lib
