<#
.SYNOPSIS
    Build script for DirectorySize module.
#>
[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = "Debug",

    [Parameter()]
    [ValidateSet("net462", "net6.0")]
    [string]
    $Framework
)

if ($Framework.Length -eq 0) {
    $Framework = if ($PSEdition -eq "Core") { "net6.0" }else { "net462" }
}

<#
.SYNOPSIS
    Build DirectorySize assembly.
#>
task BuildDirectorySize @{
    Inputs  = Get-ChildItem -Path DirectorySize\*.cs, DirectorySize\DirectorySize.csproj
    Outputs = "DirectorySize\bin\$Configuration\$Framework\DirectorySize.dll"
    Jobs    = {
        exec { dotnet publish --no-self-contained -c $Configuration -f $Framework DirectorySize }
    }
}

<#
.SYNOPSIS
    Build DirectorySize module.
#>
task BuildModule BuildDirectorySize, {
    $version = (Import-PowerShellDataFile -LiteralPath DirectorySize\DirectorySize.psd1).ModuleVersion
    $destination = "$PSScriptRoot\out\$Configuration\$Framework\DirectorySize\$version"

    if (Test-Path -LiteralPath $destination -PathType Container) {
        Remove-Item -LiteralPath $destination -Recurse
    }
    $null = New-Item -Path $destination -ItemType Directory

    $parameters = @{
        Path        = @(
            "DirectorySize\bin\$Configuration\$Framework\DirectorySize.*",
            "DirectorySize\en-US", "DirectorySize\ja-JP")
        Destination = $destination
        Recurse     = $true
    }
    Copy-Item @parameters
}

<#
.SYNOPSIS
    Install DirectorySize module.
#>
task Install BuildModule, {
    $destination = switch ($Framework) {
        "net6.0" {
            "$HOME\Documents\PowerShell\Modules"
        }
        "net462" {
            "$HOME\Documents\WindowsPowerShell\Modules"
        }
    }

    if (Test-Path -LiteralPath $destination -PathType Container) {
        if (Test-Path -LiteralPath "$destination\DirectorySize" -PathType Container) {
            Remove-Item -LiteralPath "$destination\DirectorySize" -Recurse
        }
    }
    else {
        $null = New-Item -Path $destination -ItemType Directory
    }

    Copy-Item -LiteralPath out\$Configuration\$Framework\DirectorySize -Destination $destination -Recurse
}

<#
.SYNOPSIS
    Test DirectorySize module.
#>
task Test Install, {
    $command = "& { Invoke-Pester -Path '$PSScriptRoot\test' -Output Detailed }"

    switch ($Framework) {
        "net6.0" {
            exec { pwsh -nop -c $command }
        }
        "net462" {
            exec { powershell -noprofile -command $command }
        }
    }
}

<#
.SYNOPSIS
    Run default tasks.
#>
task . Test
