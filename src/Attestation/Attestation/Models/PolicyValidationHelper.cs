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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Commands.Attestation.Models
{
    public class PolicyValidationHelper
    {
        internal static TokenValidationParameters TokenValidationParams(string tenantName)
            => new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKeyResolver = (tokenValue, securityToken, kid, validationParams) =>
                {
                    var keys = new List<SecurityKey>();
                    if (securityToken == null)
                    {
                        throw new ArgumentNullException(nameof(securityToken));
                    }

                    JsonWebToken token = securityToken as JsonWebToken;
                    if (token == null)
                    {
                        throw new ArgumentNullException(nameof(securityToken));
                    }

                    var jsonHeaderBytes = Base64Url.Decode(token.EncodedHeader);
                    var jsonHeaderString = Encoding.UTF8.GetString(jsonHeaderBytes);
                    var jsonHeader = JObject.Parse(jsonHeaderString);
                    var jkuUri = jsonHeader.SelectToken("jku");
                    Uri keyUrl = new Uri(jkuUri.ToString());

                    var webClient = new WebClient();

                    if (tenantName.Length > 24)
                    {
                        tenantName = tenantName.Remove(24);
                    }

                    // Include the tenant name in the outgoing request.
                    webClient.Headers.Add("tenantName", tenantName);
                    var jwksValue = webClient.DownloadString(keyUrl);

                    JsonWebKeySet jwks = new JsonWebKeySet(jwksValue);

                    return jwks.GetSigningKeys();
                }
            };


        public static TokenValidationResult ValidateAttestationServiceToken(string tenantName, string tenantAddress, string policyJwt, JsonWebKeySet tokenKeySet = null)
        {
            // Retrieve the signing key set for this token - we'll need it to validate the token key.
            var jwt = new JsonWebToken(policyJwt);
            var jsonHeaderBytes = Base64Url.Decode(jwt.EncodedHeader);
            var jsonHeaderString = Encoding.UTF8.GetString(jsonHeaderBytes);
            var jsonHeader = JObject.Parse(jsonHeaderString);
            var jkuUri = jsonHeader.SelectToken("jku");
            Uri keyUrl = new Uri(jkuUri.ToString());
            var webClient = new WebClient();

            // Include the tenant name in the outgoing request.
            if (tenantName.Length > 24)
            {
                tenantName = tenantName.Remove(24);
            }

            webClient.Headers.Add("tenantName", tenantName);
            var jwksValue = webClient.DownloadString(keyUrl);

            tokenKeySet = new JsonWebKeySet(jwksValue);

            // Now validate the signature using the signing keys we just read.
            TokenValidationResult validatedToken;
            {
                var tokenValidationParams = TokenValidationParams(tenantName);
                tokenValidationParams.IssuerSigningKeys = tokenKeySet.GetSigningKeys();
                tokenValidationParams.IssuerSigningKeyResolver = null;

                var jwtHandler = new JsonWebTokenHandler();

                validatedToken = jwtHandler.ValidateToken(policyJwt, tokenValidationParams);

                if (!validatedToken.IsValid)
                {
                    throw new ArgumentException("policy is not valid");
                }
            }
            // Verify that the token was signed by the attestation service on behalf of the desired tenant.
            {
                var validatedKey = validatedToken.SecurityToken.SigningKey;

                if (!(validatedKey is X509SecurityKey))
                {
                    throw new ArgumentException("SigningKey is not an X509 security ke");
                }

                var x509key = validatedKey as X509SecurityKey;
                var signingCertificate = x509key.Certificate;

                var iss = validatedToken.ClaimsIdentity.Claims.First(c => c.Type == "iss");

                if (!string.Equals(tenantAddress, iss.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("policy is not valid");
                }

                // The certificate needs to come from the same place as the token.
                if (!string.Equals(signingCertificate.Issuer, "CN=" + iss.Value,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("policy is not valid");
                }
            }
            return validatedToken;
        }

        public static bool ValidatePolicySettingToken(string tenantName, string tenantAddress, string policyJwt, string expectedPolicy, string expectedRootCertificate = default(string))
        {
            var validatedToken = ValidateAttestationServiceToken(tenantName, tenantAddress, policyJwt, null);
            if (!validatedToken.IsValid)
            {
                throw new ArgumentException("policyJwt is not valid");
            }

            // Verify that the policy hash in the token matches the expected policy hash.
            {
                var policyToVet = validatedToken.ClaimsIdentity.Claims.First(c => c.Type == "aas-policyHash");

                if (string.IsNullOrEmpty(policyToVet.Value))
                {
                    throw new ArgumentException("policyJwt is not valid");
                }

                // Verify that the policy hash is in fact the hash of the expectedPolicy.
                var policyHash = Base64Url.Decode(policyToVet.Value);

                var expectedHash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(expectedPolicy));

                if (expectedHash.Length != policyHash.Length)
                {
                    throw new ArgumentException("policyJwt is not valid");
                }

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, policyHash))
                {
                    throw new ArgumentException("policyJwt is not valid");
                }
            }

            // And if we expect a signing root certificate, verify that as well.
            {
                if (expectedRootCertificate != null)
                {
                    var policySigningCertificate = validatedToken.ClaimsIdentity.Claims.First(c => c.Type == "aas-policySigningCertificate");

                    if (string.IsNullOrEmpty(policySigningCertificate.Value))
                    {
                        throw new ArgumentException("policyJwt is not valid");
                    }

                    // Verify that the policy hash is in fact the hash of the expectedPolicy.
                    var policyCertificateJwk = policySigningCertificate.Value;

                    JsonWebKey policyCertificate = new JsonWebKey(policyCertificateJwk);

                    if (policyCertificate.X5c == null)
                    {
                        throw new ArgumentException("policyJwt is not valid");
                    }

                    if (policyCertificate.X5c.Count != 1)
                    {
                        throw new ArgumentException("policyJwt is not valid");
                    }

                    if (policyCertificate.X5c[0] != expectedRootCertificate)
                    {
                        throw new ArgumentException("policyJwt is not valid");
                    }

                }
                else
                {
                    // We didn't expect a root certificate so we shouldn't get a policy signing certificate.
                    if (validatedToken.ClaimsIdentity.Claims.FirstOrDefault(c =>
                            c.Type == "aas-policySigningCertificate") != null)
                    {
                        throw new ArgumentException("policyJwt is not valid");
                    }
                }
            }

            return true;

        }
    }
}
