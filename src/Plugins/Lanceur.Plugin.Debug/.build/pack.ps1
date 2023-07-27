param (
    [parameter(mandatory=$true)]$name,
    [parameter(mandatory=$true)]$version,
    [parameter(mandatory=$true)]$output
)


Push-Location
Write-Host "=== Pack plugin ===" -ForeGroundColor Yellow

Write-Host "`t=> Go to output directory"
Set-Location $output

Write-Host "`t=> Clean useless files"
Remove-Item *.deps.json
Remove-Item *.pdb
Remove-Item *Lanceur.Core.Plugins.dll
Remove-Item *.runtimeconfig.json
Remove-Item *.lpk

Write-Host "`t=> Zip plugin"
$fileName = $name + "_" + $version
$zip      = Join-Path -Path $output -ChildPath $fileName
Compress-Archive -Path *.* -DestinationPath "$zip.zip"

Write-Host "`t=> Change extension from '.zip' to '.lpk'"
Rename-Item "$fileName.zip" -NewName "$fileName`.lpk"

Pop-Location