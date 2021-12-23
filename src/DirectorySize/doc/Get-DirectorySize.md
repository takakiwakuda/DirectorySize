---
external help file: DirectorySize.dll-Help.xml
Module Name: DirectorySize
online version:
schema: 2.0.0
---

# Get-DirectorySize

## SYNOPSIS

Gets the size of a directory.

## SYNTAX

### Path (Default)

```powershell
Get-DirectorySize [[-Path] <String[]>] [-Recurse] [<CommonParameters>]
```

### LiteralPath

```powershell
Get-DirectorySize -LiteralPath <String[]> [-Recurse] [<CommonParameters>]
```

## DESCRIPTION

The `Get-DirectorySize` cmdlet gets the size of a directory.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-DirectorySize
```

This example gets the size of the current directory.

## PARAMETERS

### -LiteralPath

Specifies the path to a directory.

```yaml
Type: String[]
Parameter Sets: LiteralPath
Aliases: PSPath, LP

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Path

Specifies the path to a directory. Wildcard characters are permitted.

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Recurse

Gets the total size of the specified directory and all its child files.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

You can pipe a string that contains a path to `Get-DirectorySize`.

## OUTPUTS

### DirectorySize.DirectorySizeInfo

`Get-DirectorySize` returns a `DirectorySizeInfo` object that represents the size of a directory.

## NOTES

## RELATED LINKS
