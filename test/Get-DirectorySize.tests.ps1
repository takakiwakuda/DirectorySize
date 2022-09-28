Set-StrictMode -Version 3.0

Describe "Get-DirectorySize" {
    Context "Path" {
        It "Throws an exception if the specified path does not exist" {
            $er = { Get-DirectorySize -Path DoesNotExist -ErrorAction Stop } | Should -Throw -PassThru

            $er.FullyQualifiedErrorId | Should -Be "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the specified path is not filesystem" {
            $er = { Get-DirectorySize -Path TestRegistry: -ErrorAction Stop } | Should -Throw -PassThru

            $er.FullyQualifiedErrorId | Should -Be "NotFileSystemProvider,DirectorySize.GetDirectorySizeCommand"
        }

        It "Should retrieve DirectorySizeInfo objects for the path '<Path>'" -TestCases @(
            @{ Path = $PWD },
            @{ Path = $env:USERPROFILE },
            @{ Path = "$PSScriptRoot\..\*" },
            @{ Path = "DoesNotExist", $env:APPDATA }
        ) {
            $dirSizeInfo = Get-DirectorySize -Path $Path

            $dirSizeInfo | Should -BeOfType DirectorySize.DirectorySizeInfo
        }
    }

    Context "LiteralPath" {
        It "Throws an exception if the specified path does not exist" {
            $er = { Get-DirectorySize -LiteralPath DoesNotExist -ErrorAction Stop } | Should -Throw -PassThru

            $er.FullyQualifiedErrorId | Should -Be "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the specified path contains wildcard characters (*)" {
            $er = { Get-DirectorySize -LiteralPath $PSScriptRoot\..\* -ErrorAction Stop } | Should -Throw -PassThru

            $er.FullyQualifiedErrorId | Should -Be "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the specified path is not filesystem" {
            $er = { Get-DirectorySize -LiteralPath TestRegistry: -ErrorAction Stop } | Should -Throw -PassThru

            $er.FullyQualifiedErrorId | Should -Be "NotFileSystemProvider,DirectorySize.GetDirectorySizeCommand"
        }

        It "Should retrieve DirectorySizeInfo objects for the path '<Path>'" -TestCases @(
            @{ Path = $PWD },
            @{ Path = $env:USERPROFILE },
            @{ Path = "DoesNotExist", $env:APPDATA }
        ) {
            $dirSizeInfo = Get-DirectorySize -LiteralPath $Path

            $dirSizeInfo | Should -BeOfType DirectorySize.DirectorySizeInfo
        }
    }
}
