---
external help file: Microsoft.Azure.PowerShell.Cmdlets.Attestation.dll-Help.xml
Module Name: Az.Attestation
online version: https://docs.microsoft.com/en-us/powershell/module/az.attestation/set-azattestationpolicy
schema: 2.0.0
---

# Set-AzAttestationPolicy

## SYNOPSIS
Sets the policy from a tenant in Azure Attestationn.

## SYNTAX

```
Set-AzAttestationPolicy -Name <String> -Tee <String> -AttestationPolicy <String> [-PassThru]
 [-DefaultProfile <IAzureContextContainer>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-AzAttestationPolicy cmdlet sets the policy from a tenant in Azure Attestation.

## EXAMPLES

### Example 1
```powershell
PS C:\> Set-AzAttestationPolicy -Name "example" -Tee "SgxEnclave" -AttestationPolicy "policyexample"
```

Sets the user defined policy "policyexample" for tennat "example" in Tee "SgxEnclave".

## PARAMETERS

### -AttestationPolicy
Specifies the JSON Web Token describing the policy document to set.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -DefaultProfile
The credentials, account, tenant, and subscription used for communication with Azure.

```yaml
Type: IAzureContextContainer
Parameter Sets: (All)
Aliases: AzContext, AzureRmContext, AzureCredential

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Specifies a name of the tenant.
This cmdlet sets the attestation policy for the tenant that this parameter specifies.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PassThru
This Cmdlet does not return an object by default.
If this switch is specified, it returns true if successful.

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

### -Tee
Specifies a type of Trusted Execution Environment.
We support four types of environment: SgxEnclave, OpenEnclave, CyResComponent and AzureGuest.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### System.String

## NOTES

## RELATED LINKS
