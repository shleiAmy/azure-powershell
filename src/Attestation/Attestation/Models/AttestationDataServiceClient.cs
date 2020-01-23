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

            //ServiceClientCredentials clientCredentials =  AzureSession.Instance.AuthenticationFactory.GetServiceClientCredentials(context, AzureEnvironment.ExtendedEndpoint.AzureAttestationServiceEndpointResourceId);
            //attestationDataClient = AzureSession.Instance.ClientFactory.CreateCustomArmClient<AttestationClient>(clientCredentials);
            //string suffix = context.Environment.GetEndpoint(AzureEnvironment.ExtendedEndpoint.AzureAttestationServiceEndpointSuffix);
            //this.tenantUriHelper = new TenantUriHelper(suffix);

            string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InBpVmxsb1FEU01LeGgxbTJ5Z3FHU1ZkZ0ZwQSIsImtpZCI6InBpVmxsb1FEU01LeGgxbTJ5Z3FHU1ZkZ0ZwQSJ9.eyJhdWQiOiJodHRwczovL2F0dGVzdC5henVyZS5uZXQiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTc5NzIzNzM1LCJuYmYiOjE1Nzk3MjM3MzUsImV4cCI6MTU3OTcyNzYzNSwiX2NsYWltX25hbWVzIjp7Imdyb3VwcyI6InNyYzEifSwiX2NsYWltX3NvdXJjZXMiOnsic3JjMSI6eyJlbmRwb2ludCI6Imh0dHBzOi8vZ3JhcGgud2luZG93cy5uZXQvNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3L3VzZXJzL2U0ZDA2ZWU4LWNhYjctNDc1My1hOThlLTJlNmU0OTM1MjllNS9nZXRNZW1iZXJPYmplY3RzIn19LCJhY3IiOiIxIiwiYWlvIjoiQVVRQXUvOE9BQUFBdVRhb0pQTFp4T0kwa0FESnY5ZW9YTWxVNVFaV3gxTG13TTVlR3g4aUx5aHE1bTRRTlUzc2k2UWJSOHNiT2lPcEpsZUdDY3orS1VpcEwyYmdTVHoyUHc9PSIsImFtciI6WyJyc2EiLCJtZmEiXSwiYXBwaWQiOiJkMzU5MGVkNi01MmIzLTQxMDItYWVmZi1hYWQyMjkyYWIwMWMiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjRhYWIxMjIxLWVlMDktNDFjYi05ODRkLTdlNzQ2OThhNDdlYSIsImZhbWlseV9uYW1lIjoiTGVpIiwiZ2l2ZW5fbmFtZSI6IlNodWFuZ3lhbiIsImlwYWRkciI6IjEzMS4xMDcuMTQ3LjE2MyIsIm5hbWUiOiJTaHVhbmd5YW4gTGVpIiwib2lkIjoiZTRkMDZlZTgtY2FiNy00NzUzLWE5OGUtMmU2ZTQ5MzUyOWU1Iiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIxMjc1MjExODQtMTYwNDAxMjkyMC0xODg3OTI3NTI3LTM1MjIzMTc5IiwicHVpZCI6IjEwMDMyMDAwNDQ1Q0Y1MjgiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiI5MTYzMjdMaXJBSkdQSFoyNFNJT1BRa2VYTHQ5Q2ZReF9Nbm8tZ0pvTGY4IiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJzaGxlaUBtaWNyb3NvZnQuY29tIiwidXBuIjoic2hsZWlAbWljcm9zb2Z0LmNvbSIsInV0aSI6Ill6cXNNdGNVeVVHYU40UWVaWkloQUEiLCJ2ZXIiOiIxLjAifQ.w-pGV86J5uHSgkgRUJx_JyiLmWZd8yqikxSACooGiPa4wvn0oG6wn1Egqkero-C7DAPArLxq26-HMWyFyi-69SG3iP8AVpVjhsKLuikcUuOXXX92hqSj_kcmVVavWYrj44exEcy99_KUz7_zq5uzxrcAUZCZAiZkfmYedTJwK0NNpOausnZyhuGfGoQvhgVWqZD08SJye0eZYoZuV9aIrSM-CPap4fbd7xSzDQZ5wxv9tlFSXfrG1t-tPW13PJwnBnMO3GidLRFfHg6uhyt7yH35s_z_AUX3p3OwE1cvhtdr36fpql_hrpPOUyALdCNeryvWD0COFfNdobLCEsf-5g";
            AttestationCredentials credentials = new AttestationCredentials(accessToken);
            attestationDataClient = AzureSession.Instance.ClientFactory.CreateCustomArmClient<AttestationClient>(credentials);

            this.tenantUriHelper = new TenantUriHelper("us.attest.azure.net");


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
            string tenantUri = tenantAddress.EndsWith("/")? tenantAddress.Substring(0, tenantAddress.Length - 1) : tenantAddress;

            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.PrepareToSetWithHttpMessagesAsync(tenantUri, tee, PolicyJwt).Result;

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

            string policyUpdateJwt = getResult.Body.ToString();

            bool isValid = false;
            try
            {
                var validatedToken = PolicyValidationHelper.ValidateAttestationServiceToken(tenantName, tenantUri, policyUpdateJwt, null);
                if (!validatedToken.IsValid)
                {
                    throw new ArgumentException("policyJwt is not valid");
                }
                isValid = true;
            }
            catch
            {
                throw;
            }

            try
            {
                if (isValid)
                {
                    getResult = attestationDataClient.Policy.SetWithHttpMessagesAsync(tenantUri, tee, policyUpdateJwt).Result;

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
            string tenantUri = tenantAddress.EndsWith("/") ? tenantAddress.Substring(0, tenantAddress.Length - 1) : tenantAddress;

            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.ResetWithHttpMessagesAsync(tenantUri, tee, PolicyJwt).Result;

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
            string tenantUri = tenantAddress.EndsWith("/") ? tenantAddress.Substring(0, tenantAddress.Length - 1) : tenantAddress;

            AzureOperationResponse<object> getResult;
            try
            {
                getResult = attestationDataClient.Policy.GetWithHttpMessagesAsync(tenantUri, tee).Result;

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
