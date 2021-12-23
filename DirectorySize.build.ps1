[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = "Debug",

    [Parameter()]
    [ValidateSet("net6.0", "net462")]
    [string]
    $Framework
)

if ($Framework.Length -eq 0) {
    $Framework = if ($PSVersionTable.PSVersion.Major -eq 7) { "net6.0" } else { "net462" }
}

$PSModulePath = "$PSScriptRoot\out\$Configuration\$Framework\win10-x64\DirectorySize"
$PSModuleVersion = (Import-PowerShellDataFile -LiteralPath src\DirectorySize\DirectorySize.psd1).ModuleVersion
$OutputPath = "$PSModulePath\$PSModuleVersion"

task BuildDirectorySize @{
    Inputs  = Get-ChildItem -Path src\DirectorySize\*.cs, src\DirectorySize\*.csproj
    Outputs = "src\DirectorySize\bin\$Configuration\$Framework\win10-x64\DirectorySize.dll"
    Jobs    = {
        exec { dotnet publish --nologo --no-self-contained -c $Configuration -f $Framework src\DirectorySize }
    }
}

task BuildPSModule {
    if (Test-Path -LiteralPath $OutputPath -PathType Container) {
        Remove-Item -LiteralPath $OutputPath -Recurse
    }
    $null = New-Item -Path $OutputPath -ItemType Directory

    $params = @{
        Path        = @(
            "src\DirectorySize\bin\$Configuration\$Framework\win10-x64\publish\DirectorySize.*",
            "src\DirectorySize\en-US",
            "src\DirectorySize\ja-JP")
        Destination = $OutputPath
        Exclude     = "*.json", "DirectorySize.xml"
        Recurse     = $true
    }
    Copy-Item @params
}

task Test {
    $command = "& { Import-Module -Name '$PSModulePath'; Invoke-Pester -Path '$PSScriptRoot\test' }"

    switch ($Framework) {
        "net6.0" {
            exec { pwsh -NoProfile -Command $command }
        }
        "net462" {
            exec { powershell -NoProfile -Command $command }
        }
    }
}

task . BuildDirectorySize, BuildPSModule, Test
