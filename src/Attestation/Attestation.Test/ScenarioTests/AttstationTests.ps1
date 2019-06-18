<#
.SYNOPSIS
Test New-AzAttestation, Get-AzAttestation, Remove-AzAttestation
#>
#------------------------------Create-AzAttestation-----------------------------------
function Test-CreateAttestation
{
	$unknownRGName = getAssetName
	$attestationName = getAssetName
    $attestationPolicy = getAssetName

	try
	{
	    $rgName = Create-ResourceGroup
		$attestationCreated = New-AzAttestation -Name $attestationName -ResourceGroupName $rgName -AttestationPolicy $attestationPolicy
		
		Assert-NotNull attestationCreated
		Assert-AreEqual $attestationName $attestationCreated.Name
		Assert-NotNull attestationCreated.AttesUri
		Assert-NotNull attestationCreated.Id
		Assert-NotNull attestationCreated.Status
		
		# Test throws for existing attestation
		Assert-Throws { New-AzAttestation -Name $attestationName  -ResourceGroupName $rgname -AttestationPolicy $attestationPolicy}

		# Test throws for resourcegroup nonexistent
		Assert-Throws { New-AzAttestation -Name $attestationName -ResourceGroupName $unknownRGName -AttestationPolicy $attestationPolicy}
	}

	finally
	{
		Clean-ResourceGroup $$rgName.ResourceGroupName
	}
}

#------------------------------Get-AzAttestation-----------------------------------
function Test-GetAttestation
{
	$rgName = getAssetName
	$attestationName = getAssetName

	try
	{
	    $rgName = Create-ResourceGroup
		New-AzAttestation -Name $attestationName -ResourceGroupName $rgName -AttestationPolicy $attestationPolicy
		$got = Get-AzAttestationt -ResourceGroupName $rgName -Name $attestationName 

		Assert-NotNull $got
		Assert-AreEqual $got.Name $attestationName

		$unknownAttestation = getAssetName
		$unknownRG = getAssetName

		$unknown = Get-AzAttestation -ResourceGroupName $unknownRG -Name $attestationName 
		Assert-Null $unknown

		$unknown = Get-AzKeyVault -ResourceGroupName $rgName -Name $unknownAttestation 
		Assert-Null $unknown
	}

	finally
	{
		Clean-ResourceGroup $$rgName.ResourceGroupName
	}
}

#------------------------------Remove-AzAttestation-----------------------------------
function Test-DeleteAttestationByName
{
	$rgName = getAssetName
	$attestationName = getAssetName
	try
	{
	    $rgName = Create-ResourceGroup
		New-AzAttestation -Name $attestationName -ResourceGroupName $rgName -AttestationPolicy $attestationPolicy

		Remove-AzAttestation -ResourceGroupName $rgName -Name $attestationName -Force

		$deletedAttestation= Get-AzAttestationt -ResourceGroupName $rgName -Name $attestationName 
		Assert-Null $deletedAttestation

		# Test negative case
		$job = Remove-AzAttestation -ResourceGroupName $rgName -Name $attestationName -Force
		$job | Wait-Job

		Assert-Throws { $job | Receive-Job }
	}
	
	finally
	{
		Clean-ResourceGroup $$rgName.ResourceGroupName
	}
}