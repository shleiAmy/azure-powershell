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
using Microsoft.Azure.Attestation;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Management.Attestation;
using Microsoft.Azure.ServiceManagement.Common.Models;
using Microsoft.Azure.Test.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Azure.Management.Internal.Resources;
using Microsoft.Rest.ClientRuntime.Azure.TestFramework;

namespace Microsoft.Azure.Commands.Attestation.Test
{
    class AttestationController
    {
        private readonly EnvironmentSetupHelper _helper;


        public ResourceManagementClient ResourceClient { get; private set; }

        public AttestationManagementClient AttestationManagementClient { get; private set; }

        public static AttestationController NewInstance => new AttestationController();

        public AttestationController()
        {
            _helper = new EnvironmentSetupHelper();
        }


        public void RunPowerShellTest(XunitTracingInterceptor logger, params string[] scripts)
        {
            var sf = new StackTrace().GetFrame(1);
            var callingClassType = sf.GetMethod().ReflectedType?.ToString();
            var mockName = sf.GetMethod().Name;

            logger.Information(string.Format("Test method entered: {0}.{1}", callingClassType, mockName));
            _helper.TracingInterceptor = logger;

            RunPowerShellTestWorkflow(
                () => scripts,
                // no custom cleanup
                null,
                callingClassType,
                mockName,
                true,
                false);
        }

        public void RunDataPowerShellTest(XunitTracingInterceptor logger, params string[] scripts)
        {
            var sf = new StackTrace().GetFrame(1);
            var callingClassType = sf.GetMethod().ReflectedType?.ToString();
            var mockName = sf.GetMethod().Name;

            logger.Information(string.Format("Test method entered: {0}.{1}", callingClassType, mockName));
            _helper.TracingInterceptor = logger;

            RunPowerShellTestWorkflow(
                () => scripts,
                // no custom cleanup
                null,
                callingClassType,
                mockName,
                false,
                true);
        }

        public void RunPowerShellTestWorkflow(
            Func<string[]> scriptBuilder,
            Action cleanup,
            string callingClassType,
            string mockName,
            bool setupManagementClients,
            bool setupDataClient)
        {
            var providers = new Dictionary<string, string>
            {
                {"Microsoft.Resources", null},
                {"Microsoft.Features", null},
                {"Microsoft.Authorization", null}
            };
            var providersToIgnore = new Dictionary<string, string>
            {
                {"Microsoft.Azure.Management.Resources.ResourceManagementClient", "2016-02-01"},
                {"Microsoft.Azure.Management.ResourceManager.ResourceManagementClient", "2017-05-10"}
            };
            HttpMockServer.Matcher = new PermissiveRecordMatcherWithApiExclusion(true, providers, providersToIgnore);
            HttpMockServer.RecordsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SessionRecords");
            using (var context = MockContext.Start(callingClassType, mockName))
            {
                if (setupManagementClients)
                {
                    SetupManagementClients(context);
                    _helper.SetupEnvironment(AzureModule.AzureResourceManager);
                }

                if (setupDataClient)
                {
                    SetupDataClient(context);
                }

                var callingClassName =
                    callingClassType.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries).Last();
                _helper.SetupModules(AzureModule.AzureResourceManager,
                    "ScenarioTests\\Common.ps1",
                    "ScenarioTests\\" + callingClassName + ".ps1",
                    _helper.RMProfileModule,
                    _helper.GetRMModulePath("AzureRM.Attestation.psd1"),
                    "AzureRM.Resources.ps1");

                try
                {
                    var psScripts = scriptBuilder?.Invoke();
                    if (psScripts != null)
                    {
                        _helper.RunPowerShellTest(psScripts);
                    }
                }
                finally
                {
                    cleanup?.Invoke();
                }
            }
        }
        private void SetupManagementClients(MockContext context)
        {
            ResourceClient = GetResourceManagementClient(context);
            AttestationManagementClient = GetAttestationManagementClient(context);
            _helper.SetupManagementClients(ResourceClient, AttestationManagementClient);
        }

        private void SetupDataClient(MockContext context)
        {
            string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkJCOENlRlZxeWFHckdOdWVoSklpTDRkZmp6dyIsImtpZCI6IkJCOENlRlZxeWFHckdOdWVoSklpTDRkZmp6dyJ9.eyJhdWQiOiJodHRwczovL2F0dGVzdC5henVyZS5uZXQiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTc0MjAzOTI0LCJuYmYiOjE1NzQyMDM5MjQsImV4cCI6MTU3NDIwNzgyNCwiX2NsYWltX25hbWVzIjp7Imdyb3VwcyI6InNyYzEifSwiX2NsYWltX3NvdXJjZXMiOnsic3JjMSI6eyJlbmRwb2ludCI6Imh0dHBzOi8vZ3JhcGgud2luZG93cy5uZXQvNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3L3VzZXJzL2U0ZDA2ZWU4LWNhYjctNDc1My1hOThlLTJlNmU0OTM1MjllNS9nZXRNZW1iZXJPYmplY3RzIn19LCJhY3IiOiIxIiwiYWlvIjoiQVVRQXUvOE5BQUFBS0pXb3ZaNGczYUdudVZKRGhURGs5T1Fld0FkRTJ3L1h1OC90NW9VeTIweHlHUmVodDdJYVZqYzdTeElpUXZLbE9iY1JFKzJHemJIVFZFUzljSHRHd2c9PSIsImFtciI6WyJyc2EiLCJtZmEiXSwiYXBwaWQiOiJkMzU5MGVkNi01MmIzLTQxMDItYWVmZi1hYWQyMjkyYWIwMWMiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjRhYWIxMjIxLWVlMDktNDFjYi05ODRkLTdlNzQ2OThhNDdlYSIsImZhbWlseV9uYW1lIjoiTGVpIiwiZ2l2ZW5fbmFtZSI6IlNodWFuZ3lhbiIsImlwYWRkciI6IjEzMS4xMDcuMTc0LjE2MyIsIm5hbWUiOiJTaHVhbmd5YW4gTGVpIiwib2lkIjoiZTRkMDZlZTgtY2FiNy00NzUzLWE5OGUtMmU2ZTQ5MzUyOWU1Iiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIxMjc1MjExODQtMTYwNDAxMjkyMC0xODg3OTI3NTI3LTM1MjIzMTc5IiwicHVpZCI6IjEwMDMyMDAwNDQ1Q0Y1MjgiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiI5MTYzMjdMaXJBSkdQSFoyNFNJT1BRa2VYTHQ5Q2ZReF9Nbm8tZ0pvTGY4IiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJzaGxlaUBtaWNyb3NvZnQuY29tIiwidXBuIjoic2hsZWlAbWljcm9zb2Z0LmNvbSIsInV0aSI6IkVBV0QxMjJMRTBLVV9WSG5VcEpEQUEiLCJ2ZXIiOiIxLjAifQ.U75OytYrc6eRRu70lt2cuLKLgiDLPqKtwxFf5VPaGs9wGiR190O_FqzOAtB6YXXkrj_g-8rtNG5sOEZgLMpxUiN1wC66Qe4n6GJytlYmg9tsALwV5MoY-kchNR4ZRTIzWX42b6mqva7wzzA-AKCTAfhF-Ktzi8QdHY1QNoSouoSPdgm6e5AD9M1VYw9BVWnvwy3twSLXDNPPLn_snIwgKYJZPsDovw-IsXjzXkg0wtUIIQpYtsQk1yi0__Rx5fxq_pIKIpjHWcmRIzKZ5G5mOsH3oycrz7scSIl5Z4xWb-S0cVGPrHcdOVFVc4ySaJ-NyH3Qo0P8-ckUzJZxIuhCtw";
            AttestationCredentials credentials = new AttestationCredentials(accessToken);
            var attestationDataClient = new AttestationClient(credentials, HttpMockServer.CreateInstance());
            _helper.SetupManagementClients(attestationDataClient);
        }
        private static ResourceManagementClient GetResourceManagementClient(MockContext context)
        {
            return context.GetServiceClient<ResourceManagementClient>(TestEnvironmentFactory.GetTestEnvironment());
        }

        private static AttestationManagementClient GetAttestationManagementClient(MockContext context)
        {
            return context.GetServiceClient<AttestationManagementClient>(TestEnvironmentFactory.GetTestEnvironment());
        }
    }
}
