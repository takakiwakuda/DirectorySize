#Requires -Module DirectorySize

#region using directive
using namespace DirectorySize
#endregion

Set-StrictMode -Version 3.0

Describe "Get-DirectorySize" {
    Context "Path" {
        It "Throws an exception if the specified path does not exist" {
            { Get-DirectorySize -Path TestDrive:\Nuko.txt -ErrorAction Stop } |
                Should -Throw -ErrorId "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the root of the specified path does not exist" {
            { Get-DirectorySize -Path NukoDrive: -ErrorAction Stop } |
                Should -Throw -ErrorId "DriveNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the specified path is not a FileSystemProvider" {
            { Get-DirectorySize -Path TestRegistry: -ErrorAction Stop } |
                Should -Throw -ErrorId "NotFileSystemProvider,DirectorySize.GetDirectorySizeCommand"
        }

        It "Should return nothing if the specified path is a file" {
            Get-DirectorySize -Path $PSCommandPath | Should -BeNullOrEmpty
        }

        It "Should return a DirectorySizeInfo object" {
            $directory = Get-DirectorySize -Path $PSScriptRoot
            $excepted = Get-ChildItem -LiteralPath $PSScriptRoot | Measure-Object -Property Length -Sum

            $directory | Should -BeOfType ([DirectorySizeInfo])
            $directory.Files | Should -Be $excepted.Count
            $directory.Length | Should -Be $excepted.Sum
        }

        It "Should return a DirectorySizeInfo object with the Recurse parameter" {
            $path = "$PSScriptRoot\.."
            $directory = Get-DirectorySize -Path $path -Recurse
            $excepted = Get-ChildItem -LiteralPath $path -Recurse -Force | Measure-Object -Property Length -Sum

            $directory | Should -BeOfType ([DirectorySizeInfo])
            $directory.Files | Should -Be $excepted.Count
            $directory.Length | Should -Be $excepted.Sum
        }

        It "Should return a DirectorySizeInfo object even if the specified path contains wildcard characters" {
            $path = "$PSScriptRoot\..\tes*"
            $directory = Get-DirectorySize -Path $path
            $excepted = Resolve-Path -Path $path | Get-ChildItem | Measure-Object -Property Length -Sum

            $directory | Should -BeOfType ([DirectorySizeInfo])
            $directory.Files | Should -Be $excepted.Count
            $directory.Length | Should -Be $excepted.Sum
        }
    }

    Context "LiteralPath" {
        It "Throws an exception if the specified path does not exist" {
            { Get-DirectorySize -LiteralPath TestDrive:\Nuko.txt -ErrorAction Stop } |
                Should -Throw -ErrorId "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the root of the specified path does not exist" {
            { Get-DirectorySize -LiteralPath NukoDrive: -ErrorAction Stop } |
                Should -Throw -ErrorId "DriveNotFound,DirectorySize.GetDirectorySizeCommand"
        }

        It "Throws an exception if the specified path is not a FileSystemProvider" {
            { Get-DirectorySize -LiteralPath TestRegistry: -ErrorAction Stop } |
                Should -Throw -ErrorId "NotFileSystemProvider,DirectorySize.GetDirectorySizeCommand"
        }

        It "Should return nothing if the specified path is a file" {
            Get-DirectorySize -LiteralPath $PSCommandPath | Should -BeNullOrEmpty
        }

        It "Should return a DirectorySizeInfo object with a Length property" {
            $directory = Get-DirectorySize -LiteralPath $PSScriptRoot
            $excepted = Get-ChildItem -LiteralPath $PSScriptRoot | Measure-Object -Property Length -Sum

            $directory | Should -BeOfType ([DirectorySizeInfo])
            $directory.Files | Should -Be $excepted.Count
            $directory.Length | Should -Be $excepted.Sum
        }

        It "Should return a DirectorySizeInfo object with the Recurse parameter" {
            $path = "$PSScriptRoot\.."
            $directory = Get-DirectorySize -LiteralPath $path -Recurse
            $excepted = Get-ChildItem -LiteralPath $path -Force -Recurse | Measure-Object -Property Length -Sum

            $directory | Should -BeOfType ([DirectorySizeInfo])
            $directory.Files | Should -Be $excepted.Count
            $directory.Length | Should -Be $excepted.Sum
        }

        It "Throws an exception if the specified path contains wildcard characters" {
            { Get-DirectorySize -LiteralPath "$PSScriptRoot\..\tes*" -ErrorAction Stop } |
                Should -Throw -ErrorId "PathNotFound,DirectorySize.GetDirectorySizeCommand"
        }
    }
}
