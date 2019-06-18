﻿// ----------------------------------------------------------------------------------
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

using Microsoft.Azure.Commands.Attestation.Models;
using Microsoft.Azure.Commands.Attestation.Properties;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using System;
using Microsoft.Rest.Azure;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Attestation
{
    [Cmdlet("Remove", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "Attestation", SupportsShouldProcess = true, DefaultParameterSetName = NameParameterSet)]
    [OutputType(typeof(bool))]
    public class RemoveAzureAttestation : AttestationManagementCmdletBase
    {
        #region Parameter Set Names

        private const string NameParameterSet = "ByAvailableAttestation"; //default
        private const string ResourceIdParameterSet = "ResourceIdByAvailableAttestation";
        private const string InputObjectParameterSet = "InputObjectByAvailableAttestation";


        #endregion

        #region Input Parameter Definitions

        /// <summary>
        /// Resource group to which the attestation belongs.
        /// </summary>
        [Parameter(Mandatory = false,
            Position = 0,
            ParameterSetName = NameParameterSet,
            HelpMessage = "Specifies the name of resource group for Azure attestation to remove.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty()]
        public string ResourceGroupName { get; set; }


        /// <summary>
        /// Attestation object
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 0,
            ParameterSetName = ResourceIdParameterSet,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Attestation Resource Id.")]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }


        /// <summary>
        /// Attestation object
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 0,
            ParameterSetName = InputObjectParameterSet,
            ValueFromPipeline = true,
            HelpMessage = "Attestation object to be deleted.")]
        [ValidateNotNullOrEmpty]
        public PSAttestation InputObject { get; set; }


        /// <summary>
        /// Attestation name
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 1,
            ParameterSetName = NameParameterSet,
            HelpMessage = "Specifies the name of the attestation to remove.")]
        [ResourceNameCompleter("Microsoft.Attestation", "ResourceGroupName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }


        /// <summary>
        /// If present, do not ask for confirmation
        /// </summary>
        [Parameter(Mandatory = false,
        HelpMessage = "Indicates that the cmdlet does not prompt you for confirmation. By default, this cmdlet prompts you to confirm that you want to delete the attestation.")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        [Parameter(Mandatory = false,
            HelpMessage = "This Cmdlet does not return an object by default. If this switch is specified, it returns true if successful.")]
        public SwitchParameter PassThru { get; set; }
        #endregion

        public override void ExecuteCmdlet()
        {
            if (InputObject != null)
            {
                Name = InputObject.AttestationName;
                ResourceGroupName = InputObject.ResourceGroupName;
            }
            else if (ResourceId != null)
            {
                var resourceIdentifier = new ResourceIdentifier(ResourceId);
                Name = resourceIdentifier.ResourceName;
                ResourceGroupName = resourceIdentifier.ResourceGroupName;
            }

            if (string.IsNullOrEmpty(ResourceGroupName))
            {
                throw new CloudException(string.Format("ResourceGroupNotSpecified", Name));
            }

            //else if (ResourceId != null)
            //{
            //    var resourceIdentifier = new ResourceIdentifier(ResourceId);
            //    Name = resourceIdentifier.ResourceName;
            //    ResourceGroupName = resourceIdentifier.ResourceGroupName;
            //}

            //if (ShouldProcess(clusterName, Resources.RemovingKustoCluster))
            //i see other module using the similar syntax, don't know what's this check
            if (string.IsNullOrEmpty(Name))
            {
                throw new CloudException(string.Format("ResourceNotSpecified", Name));
            }

            //also need to dicusses with teams to check if the resource group is mandatory
            if (string.IsNullOrEmpty(ResourceGroupName))
            {
                throw new CloudException(string.Format("ResourceGroupNotSpecified", ResourceGroupName));
            }


            PSAttestation attestation = null;
            if (!AttestationClient.CheckIfAttestationExists(ResourceGroupName, Name, out attestation))
            {
                throw new InvalidOperationException(string.Format("AttestationDoesNotExist", Name));
            }
            AttestationClient.DeleteAttestation( Name,ResourceGroupName);

            if (PassThru.IsPresent)
            {
                WriteObject(true);
            }
        }
    }
}
