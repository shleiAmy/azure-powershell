// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Commands.Attestation.Models;

namespace Microsoft.Azure.Commands.Attestation
{
    /// <summary>
    /// Reset AttestationPolicy.
    /// </summary>
    [Cmdlet("Reset", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "AttestationPolicy", SupportsShouldProcess = true)]
    [OutputType(typeof(bool))]
    public class ResetAzureAttestationPolicy : AttestationDataServiceCmdletBase
    {
        #region Input Parameter Definitions

        /// <summary>
        /// Tenant name
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage =
                "Specifies a name of the tenant. This cmdlet resets the attestation policy for the tenant that this parameter specifies."
        )]
        [ValidateNotNullOrEmpty]
        [Alias("InstanceName")]
        public string Name { get; set; }

        /// <summary>
        /// Trusted Execution Environment
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage =
                "Specifies a type of Trusted Execution Environment. We support four types of environment: SgxEnclave, OpenEnclave, CyResComponent and AzureGuest."
        )]
        [ValidateNotNullOrEmpty]
        public string Tee { get; set; }

        /// <summary>
        /// JSON Web Token
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage =
                "Specifies the JSON Web Token describing the policy document to reset."
        )]
        [ValidateNotNullOrEmpty]
        public string AttestationPolicy { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "This Cmdlet does not return an object by default. If this switch is specified, it returns true if successful.")]
        public SwitchParameter PassThru { get; set; }

        #endregion

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(Name, "ResetAttestationPolicy"))
            {
                AttestationDataClient.DeletePolicy(Name, Tee, AttestationPolicy);
                if (PassThru)
                {
                    WriteObject(true);
                }
            }
        }
    }
}
