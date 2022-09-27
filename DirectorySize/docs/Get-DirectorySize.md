---
external help file: DirectorySize.dll-Help.xml
Module Name: DirectorySize
online version: https://github.com/takakiwakuda/DirectorySize/blob/main/DirectorySize/docs/Get-DirectorySize.md
schema: 2.0.0
---

# Get-DirectorySize

## SYNOPSIS

Retrieves the total size of files in a specified directory.

## SYNTAX

### Path (Default)

```powershell
Get-DirectorySize [[-Path] <String[]>] [<CommonParameters>]
```

### LiteralPath

```powershell
Get-DirectorySize [-LiteralPath <String[]>] [<CommonParameters>]
```

## DESCRIPTION

The `Get-DirectorySize` cmdlet retrieves the total size of files in a specified directory.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-DirectorySize -Path 'C:\Example'
```

This example retrieves the total size of files in the directory `C:\Example`.

## PARAMETERS

### -LiteralPath

Specifies paths to directories. No characters are interpreted as wildcard characters.

```yaml
Type: String[]
Parameter Sets: LiteralPath
Aliases: PSPath, LP

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Path

Specifies paths to directories. Wildcard characters are permitted.

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### DirectorySize.DirectorySizeInfo

## NOTES

## RELATED LINKS
