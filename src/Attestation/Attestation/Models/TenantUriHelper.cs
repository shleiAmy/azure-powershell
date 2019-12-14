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

        public string GetTenantName(string tenantAddress)
        {
            Uri tenantUri = CreateAndValidateTenantUri(tenantAddress);
            return tenantUri.Host.Split('.').First();
        }

        public String CreateTenantAddress(string tenantName)
        {
            return CreateTenantUri(tenantName).ToString();
        }

        public string TenantDnsSuffix { get; private set; }

        private Uri CreateAndValidateTenantUri(string tenantAddress)
        {
            if (string.IsNullOrEmpty(tenantAddress))
                throw new ArgumentNullException("tenantAddress");

            Uri tenantUri;
            if (!Uri.TryCreate(tenantAddress, UriKind.Absolute, out tenantUri))
                throw new ArgumentException(string.Format("InvalidTennatUri", tenantAddress, this.TenantDnsSuffix));

            if (tenantUri.HostNameType != UriHostNameType.Dns ||
                !tenantUri.Host.EndsWith(this.TenantDnsSuffix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(string.Format("InvalidTennatUri", tenantAddress, this.TenantDnsSuffix));

            return tenantUri;
        }

        private Uri CreateTenantUri(string tenantName)
        {
            if (string.IsNullOrEmpty(tenantName))
                throw new ArgumentNullException("tenantName");

            UriBuilder builder = new UriBuilder("https", tenantName + "." + this.TenantDnsSuffix);

            return builder.Uri;
        }
    }
}
