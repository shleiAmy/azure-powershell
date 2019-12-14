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
using System.Net;
using Microsoft.Azure.Attestation;
using Microsoft.Azure.Attestation.Models;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace Microsoft.Azure.Commands.Attestation.Models
{
    public class AttestationDataServiceClient
    {
        private TenantUriHelper tenantUriHelper;
        private readonly AttestationClient attestationDataClient;
        public AttestationDataServiceClient(IAuthenticationFactory authFactory, IAzureContext context)
        {
            if (authFactory == null)
                throw new ArgumentNullException(nameof(authFactory));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Environment == null)
                throw new ArgumentException("Invalid Azure Environment");

            ServiceClientCredentials clientCredentials =  AzureSession.Instance.AuthenticationFactory.GetServiceClientCredentials(context, AzureEnvironment.ExtendedEndpoint.AzureAttestationServiceEndpointResourceId);
            //ServiceClientCredentials clientCredentials = AzureSession.Instance.AuthenticationFactory.GetServiceClientCredentials(context, "https://attest.azure.net");
            
            attestationDataClient = AzureSession.Instance.ClientFactory.CreateCustomArmClient<AttestationClient>(clientCredentials);

            string suffix = context.Environment.GetEndpoint(AzureEnvironment.ExtendedEndpoint.AzureAttestationServiceEndpointSuffix);
            this.tenantUriHelper = new TenantUriHelper("attest.azure.net");
        }

        public void SetPolicy(string tenantName, string tee, string PolicyJwt)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            if (string.IsNullOrEmpty(tee))
            {
                throw new ArgumentNullException(nameof(tee));
            }

            if (string.IsNullOrEmpty(PolicyJwt))
            {
                throw new ArgumentNullException(nameof(PolicyJwt));
            }

            string tenantAddress = this.tenantUriHelper.CreateTenantAddress(tenantName);
            //string tenantAddress = "https://tradewinds.us.test.attest.azure.net:443";

            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.SetWithHttpMessagesAsync(tenantAddress, tee, PolicyJwt).Result;

                if (getResult.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException("Operation returns BadRequest");
                }

                if (getResult.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ArgumentException("Operation is unauthorized");
                }
            }
            catch
            {
                throw;
            }

            string policyUpdateJwt = getResult.ToString();

            bool isValid = false;
            try
            {
                isValid = PolicyValidationHelper.ValidatePolicySettingToken(tenantName, tenantAddress, PolicyJwt,
                    policyUpdateJwt);
            }
            catch
            {
                throw;
            }

            try
            {
                if (isValid)
                {
                    getResult = attestationDataClient.Policy.SetWithHttpMessagesAsync(tenantAddress, tee, policyUpdateJwt).Result;

                    if (getResult.Response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new ArgumentException("Operation returns BadRequest");
                    }

                    if (getResult.Response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new ArgumentException("Operation is unauthorized");
                    }
                }  
            }
            catch 
            {
                throw;
            }

        }

        public void DeletePolicy(string tenantName, string tee, string PolicyJwt)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            if (string.IsNullOrEmpty(tee))
            {
                throw new ArgumentNullException(nameof(tee));
            }

            if (string.IsNullOrEmpty(PolicyJwt))
            {
                throw new ArgumentNullException(nameof(PolicyJwt));
            }

            string tenantAddress = this.tenantUriHelper.CreateTenantAddress(tenantName);
            //string tenantAddress = "https://tradewinds.us.test.attest.azure.net:443";

            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.ResetWithHttpMessagesAsync(tenantAddress, tee, PolicyJwt).Result;

                if (getResult.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException("Operation returns BadRequest");
                }

                if (getResult.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ArgumentException("Operation is unauthorized");
                }
            }
            catch
            {
                throw;
            }
        }

        public string  GetPolicy(string tenantName, string tee)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            if (string.IsNullOrEmpty(tee))
            {
                throw new ArgumentNullException(nameof(tee));
            }

            string tenantAddress = this.tenantUriHelper.CreateTenantAddress(tenantName);
            //string tenantAddress = "https://tradewinds.us.test.attest.azure.net:443";

            //var getSgxPolicy = attestationDataClient.Policy.Get(tenantAddress, tee);
            //return ((AttestationPolicy)getSgxPolicy).Policy;
            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.GetWithHttpMessagesAsync(tenantAddress, tee).Result;

                if (getResult.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException("Operation returns BadRequest");
                }

                if (getResult.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ArgumentException("Operation is unauthorized");
                }
                return ((AttestationPolicy)getResult.Body).Policy;
            }
            catch
            {
                throw;
            }
        }
    }
}
