﻿using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Graph.RBAC.Version1_6;
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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;

namespace Microsoft.Azure.Commands.Attestation.Test
{
    class AttestationController
    {
        private readonly EnvironmentSetupHelper _helper;

        //private const string TenantIdKey = "TenantId";
        //private const string DomainKey = "Domain";
        //private const string SubscriptionIdKey = "SubscriptionId";

        public ResourceManagementClient ResourceClient { get; private set; }

        public AttestationManagementClient AttestationManagementClient { get; private set; }

        //public GraphRbacManagementClient GraphClient { get; private set; }

        //public string UserDomain { get; private set; }

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
                mockName);
        }

        public void RunPowerShellTestWorkflow(
            Func<string[]> scriptBuilder,
            Action cleanup,
            string callingClassType,
            string mockName)
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
                SetupManagementClients(context);
                _helper.SetupEnvironment(AzureModule.AzureResourceManager);//shouldn't miss this setting
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
            //GraphClient = GetGraphClient(context);
            AttestationManagementClient = GetAttestationManagementClient(context);
            _helper.SetupManagementClients(ResourceClient, AttestationManagementClient);
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
