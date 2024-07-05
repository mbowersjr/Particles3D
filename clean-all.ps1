[CmdletBinding(SupportsShouldProcess)]
Param (
    [Parameter(Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [String] $BuildDirectoryName = "artifacts"
)

$solutionDirectory = $PSScriptRoot
Write-Host "Cleaning build output directories under ""$solutionDirectory"" ..."

$binDirectories = Get-ChildItem -Path $solutionDirectory -Recurse -Directory -Filter "bin"
Write-Host "$($binDirectories.Count) ""bin"" directories found ..."

foreach ($dir in $binDirectories) {
    Write-Host " - $($dir.FullName)"
    Remove-Item -Path $dir.FullName -Recurse -Force -WhatIf:([bool]$WhatIfPreference.IsPresent)
}

$objDirectories = Get-ChildItem -Path $solutionDirectory -Recurse -Directory -Filter "obj"
Write-Host "$($objDirectories.Count) ""obj"" directories found ..."

foreach ($dir in $objDirectories) {
    Write-Host " - $($dir.FullName)"
    Remove-Item -Path $dir.FullName -Recurse -Force -WhatIf:([bool]$WhatIfPreference.IsPresent)
}

$publishDirectory = Join-Path -Path $PSScriptRoot -ChildPath "publish"
if (Test-Path -Path $publishDirectory) {
    Write-Host "Cleaning publish directory ""$publishDirectory"" ..."
    Remove-Item -Path $publishDirectory -Recurse -Force -WhatIf:([bool]$WhatIfPreference.IsPresent)
}

$buildDirectory = Join-Path -Path $PSScriptRoot -ChildPath $BuildDirectoryName
if (Test-Path -Path $buildDirectory) {
    Write-Host "Cleaning build directory ""$buildDirectory"" ..."
    Remove-Item -Path $buildDirectory -Recurse -Force -WhatIf:([bool]$WhatIfPreference.IsPresent)
}
