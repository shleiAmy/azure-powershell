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

using System.Collections;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Internal.Resources.Utilities;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using ResourceManagement = Microsoft.Azure.Management.Internal.Resources.Models;

namespace Microsoft.Azure.Commands.Attestation.Models
{
    public class PSAttestationIdentityItem
    {
        public PSAttestationIdentityItem()
        {

        }
        public PSAttestationIdentityItem(ResourceManagement.GenericResource resource)
        {
            ResourceIdentifier identifier = new ResourceIdentifier(resource.Id);
            AttestationName = identifier.ResourceName;
            ResourceId = resource.Id;
            ResourceGroupName = identifier.ResourceGroupName;
            SubscriptionId = identifier.Subscription;
        }
        public string ResourceId { get; protected set; }

        public string AttestationName { get; protected set; }

        public string ResourceGroupName { get; protected set; }

        public string SubscriptionId { get; protected set; }

        //public string AttestationPolicyName { get;protected set; }

    }
}
