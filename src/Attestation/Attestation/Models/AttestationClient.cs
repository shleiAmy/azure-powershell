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

// TODO: Remove IfDef
#if NETSTANDARD
using Microsoft.Azure.Graph.RBAC.Version1_6.ActiveDirectory;
#else
using Microsoft.Azure.ActiveDirectory.GraphClient;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Attestation;
using PSAttestationProperties = Microsoft.Azure.Commands.Attestation.Properties;
using Microsoft.Azure.Management.Attestation.Models;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Commands.Attestation.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.Attestation.Models
{
    public class AttestationClient
    {
        private readonly AttestationManagementClient attestationClient;

        public AttestationClient(IAzureContext context)
        {
            _subscriptionId = context.Subscription.GetId();
            attestationClient = AzureSession.Instance.ClientFactory.CreateArmClient<AttestationManagementClient>(context, AzureEnvironment.Endpoint.ResourceManager);
        }

        public AttestationClient()
        {
        }

        //private IAttestationManagementClient attestationClient { get; set; }
        private readonly Guid _subscriptionId;
        public PSAttestation CreateNewAttestation(AttestationCreationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (string.IsNullOrWhiteSpace(parameters.ProviderName))
                throw new ArgumentNullException("parameters.ProviderName");
            if (string.IsNullOrWhiteSpace(parameters.ResourceGroupName))
                throw new ArgumentNullException("parameters.ResourceGroupName");

            var response = attestationClient.AttestationProviders.Create(
                resourceGroupName: parameters.ResourceGroupName,
                providerName: parameters.ProviderName,
                creationParams: new AttestationServiceCreationParams { AttestationPolicy = JsonConvert.SerializeObject(parameters.AttestationPolicy) }
                //creationParams: new AttestationServiceCreationParams { AttestationPolicy = parameters.AttestationPolicy}
                );

            //return new PSAttestation(response);

                //Or just return this AsyncCallback, but depends on what's the input parameters of CreateOrUpdate function
                //return this.AttestationResourceProvider.Attestation.CreateOrUpdate(
                //    resourceGroupName: parameters.ResourceGroupName,
                //    containerGroupName: parametersame); //thre should be 3 parameters

            return new PSAttestation(response);
        }

        public PSAttestation GetAttestation(string attestationName, string resourceGroupName)
        {
            if (string.IsNullOrWhiteSpace(attestationName))
                throw new ArgumentNullException("attestationName");
            try
            {
                var response = attestationClient.AttestationProviders.Get(resourceGroupName, attestationName);
                return new PSAttestation(response);
            }
            catch (CloudException ce)
            {
                if (ce.Response.StatusCode == HttpStatusCode.NoContent ||
                    ce.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }       
        }
        public void DeleteAttestation(string attestationName, string resourceGroupName)
        {
            if (string.IsNullOrWhiteSpace(attestationName))
                throw new ArgumentNullException("attestationName");
            //if (string.IsNullOrWhiteSpace(resourceGroupName))
            //    throw new ArgumentNullException("resourceGroupName");

            try
            {
                //depends on how the attestation pr code is written 
                attestationClient.AttestationProviders.Delete(resourceGroupName, attestationName);
            }
            catch (CloudException ce)
            {
                if (ce.Response.StatusCode == HttpStatusCode.NoContent || ce.Response.StatusCode == HttpStatusCode.NotFound)
                    throw new ArgumentException(string.Format("AttestationNotFound", attestationName, resourceGroupName));
                throw;
            }
        }
        //public readonly string AttestationResourceType = "Microsoft.Attestation";

        public bool CheckIfAttestationExists(string resourceGroupName, string attestationName, out PSAttestation attestation)
        {
            try
            {
                attestation = GetAttestation(resourceGroupName, attestationName);
                return true;
            }
            catch (CloudException ex)
            {
                if ((ex.Response != null && ex.Response.StatusCode == HttpStatusCode.NotFound) || ex.Message.Contains(string.Format("FailedToDiscoverResourceGroup", attestationName,
                        _subscriptionId)))
                {
                    attestation = null;
                    return false;
                }

                throw;
            }
        }

    }
}