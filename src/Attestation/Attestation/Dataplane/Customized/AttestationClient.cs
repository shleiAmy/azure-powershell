// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using Microsoft.Rest;
using Microsoft.Rest.Azure;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Attestation
{
    public partial class AttestationClient : ServiceClient<AttestationClient>, IAttestationClient, IAzureClient
    {

        /// <summary>
        /// The authentication callback delegate which is to be implemented by the client code
        /// </summary>
        /// <param name="authority"> Identifier of the authority, a URL. </param>
        /// <param name="resource"> Identifier of the target resource that is the recipient of the requested token, a URL. </param>
        /// <param name="scope"> The scope of the authentication request. </param>
        /// <returns> access token </returns>
        public delegate Task<string> AuthenticationCallback(string authority, string resource, string scope);


        /// <summary>
        /// Initializes a new instance of the AttestationClient class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Subscription credentials which uniquely identify client subscription.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        public AttestationClient(ServiceClientCredentials credentials)
            : this(credentials, (DelegatingHandler[])null)
        {
        }

        partial void CustomInitialize()
        {
            var firstHandler = this.FirstMessageHandler as DelegatingHandler;
            if (firstHandler == null) return;

            var customHandler = new CustomDelegatingHandler
            {
                InnerHandler = firstHandler.InnerHandler,
                Client = this,
            };

            firstHandler.InnerHandler = customHandler;
        }
    }
}
