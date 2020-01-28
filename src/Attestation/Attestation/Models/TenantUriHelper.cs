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
using System.Linq;
using AttestationProperties = Microsoft.Azure.Commands.Attestation.Properties;

namespace Microsoft.Azure.Commands.Attestation.Models
{
    internal class TenantUriHelper
    {
        public TenantUriHelper(string tenantDnsSuffix)
        {
            if (string.IsNullOrEmpty(tenantDnsSuffix))
                throw new ArgumentNullException("tenantDnsSuffix");
            this.TenantDnsSuffix = tenantDnsSuffix;
        }

        public String CreateTenantAddress(string tenantName)
        {
            return CreateTenantUri(tenantName).ToString();
        }

        public string TenantDnsSuffix { get; private set; }

        private Uri CreateTenantUri(string tenantName)
        {
            if (string.IsNullOrEmpty(tenantName))
                throw new ArgumentNullException("tenantName");
            UriBuilder builder = new UriBuilder("https", tenantName + "." + this.TenantDnsSuffix);
            return builder.Uri;
        }
    }
}
