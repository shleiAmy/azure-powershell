# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

<#
.SYNOPSIS
Test New-AzAttestation
#>
#------------------------------Create-AzAttestation-----------------------------------
function Test-CreateAttestation
{
	$unknownRGName = getAssetName
	$attestationName = getAssetName
    $attestationPolicyName = "SgxDisableDebugMode"

	try
	{
	    $rgName = Create-ResourceGroup
		$attestationCreated = New-AzAttestation -Name $attestationName -ResourceGroupName $rgName.ResourceGroupName -AttestationPolicyName $attestationPolicyName
		
		Assert-NotNull attestationCreated
		Assert-AreEqual $attestationName $attestationCreated.Name
		Assert-NotNull attestationCreated.AttesUri
		Assert-NotNull attestationCreated.Id
		Assert-NotNull attestationCreated.Status
		
		# Test throws for existing attestation
		Assert-Throws { New-AzAttestation -Name $attestationName  -ResourceGroupName $rgName.ResourceGroupName -AttestationPolicyName $attestationPolicyName}

		# Test throws for resourcegroup nonexistent
		Assert-Throws { New-AzAttestation -Name $attestationName -ResourceGroupName $unknownRGName -AttestationPolicyName $attestationPolicyName}
	}

	finally
	{
		Clean-ResourceGroup $rgName.ResourceGroupName
	}
}

function Test-CreateAttestationWithPolicySigningCertificate
{
	$unknownRGName = getAssetName
	$attestationName = getAssetName
	$attestationPolicyName = "SgxDisableDebugMode"
	$TestOutputRoot = [System.AppDomain]::CurrentDomain.BaseDirectory;
	$PemDir = Join-Path $TestOutputRoot "PemFiles"
        $file= Join-Path $PemDir "policySigningCerts.pem"

	try
	{
	    $rgName = Create-ResourceGroup
		$attestationCreated = New-AzAttestation -Name $attestationName -ResourceGroupName $rgName.ResourceGroupName -PolicySigningCertificateFile $file
		
		Assert-NotNull attestationCreated
		Assert-AreEqual $attestationName $attestationCreated.Name
		Assert-NotNull attestationCreated.AttesUri
		Assert-NotNull attestationCreated.Id
		Assert-NotNull attestationCreated.Status
		
		# Test throws for existing attestation
		Assert-Throws { New-AzAttestation -Name $attestationName  -ResourceGroupName $rgName.ResourceGroupName -AttestationPolicyName $attestationPolicyName}

		# Test throws for resourcegroup nonexistent
		Assert-Throws { New-AzAttestation -Name $attestationName -ResourceGroupName $unknownRGName -AttestationPolicyName $attestationPolicyName}
	}

	finally
	{
		Clean-ResourceGroup $rgName.ResourceGroupName
	}
}

<#
.SYNOPSIS
Test Get-AzAttestation
#>
#------------------------------Get-AzAttestation-----------------------------------
function Test-GetAttestation
{	
	$attestationName = getAssetName
	$attestationPolicyName = "SgxDisableDebugMode"
	try
	{
	    $rgName = Create-ResourceGroup
		New-AzAttestation -Name $attestationName -ResourceGroupName $rgName.ResourceGroupName -AttestationPolicyName $attestationPolicyName

		$got = Get-AzAttestation  -Name $attestationName  -ResourceGroupName $rgName.ResourceGroupName
		Assert-NotNull got
		Assert-AreEqual $attestationName $got.Name
	}

	finally
	{
		Clean-ResourceGroup $rgName.ResourceGroupName
	}
}

<#
.SYNOPSIS
Test Remove-AzAttestation
#>
#------------------------------Remove-AzAttestation-----------------------------------
function Test-DeleteAttestationByName
{
	$attestationName = getAssetName
	$attestationPolicyName = "SgxDisableDebugMode"
	try
	{
		$rgName = Create-ResourceGroup
		New-AzAttestation -Name $attestationName -ResourceGroupName $rgName.ResourceGroupName -AttestationPolicyName $attestationPolicyName
		Remove-AzAttestation  -Name $attestationName  -ResourceGroupName $rgName.ResourceGroupName 

		Assert-Throws {Get-AzAttestation  -Name $attestationName  -ResourceGroupName $rgName.ResourceGroupName}
	}
	
	finally
	{
		Clean-ResourceGroup $rgName.ResourceGroupName
	}
}
