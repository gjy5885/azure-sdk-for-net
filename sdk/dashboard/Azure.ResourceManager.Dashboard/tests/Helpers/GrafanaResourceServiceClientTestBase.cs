// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.TestFramework;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.TestFramework;
using Azure.ResourceManager.Dashboard.Models;
using Azure.ResourceManager.Resources;

namespace Azure.ResourceManager.Dashboard.Tests.Helpers
{
    [RunFrequency(RunTestFrequency.Manually)]
    public class GrafanaResourceServiceClientTestBase : ManagementRecordedTestBase<DashboardManagementTestEnvironment>
    {
        private const string dummySSHKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQC+wWK73dCr+jgQOAxNsHAnNNNMEMWOHYEccp6wJm2gotpr9katuF/ZAdou5AaW1C61slRkHRkpRRX9FA9CYBiitZgvCCz+3nWNN7l/Up54Zps/pHWGZLHNJZRYyAB6j5yVLMVHIHriY49d/GZTZVNB8GoJv9Gakwc/fuEZYYl4YDFiGMBP///TzlI4jhiJzjKnEvqPFki5p2ZRJqcbCiF4pJrxUQR/RXqVFQdbRLZgYfJ8xGB878RENq3yQ39d8dVOkq4edbkzwcUmwwwkYVPIoDGsYLaRHnG+To7FvMeyO7xDVQkMKzopTQV8AuKpyvpqu0a9pWOMaiCyDytO7GGN you@me.com";
        public GrafanaResourceServiceClientTestBase(bool isAsync, RecordedTestMode record) : base(isAsync, RecordedTestMode.Record)
        {
        }

        public bool IsTestTenant = false;
        public static TimeSpan ZeroPollingInterval { get; } = TimeSpan.FromSeconds(0);
        public Dictionary<string, string> Tags { get; internal set; }

        public ArmClient ArmClient { get; set; }

        public SubscriptionResource Subscription
        {
            get
            {
                return ArmClient.GetDefaultSubscription();
            }
        }

        public Resources.ResourceGroupResource ResourceGroup
        {
            get
            {
                return Subscription.GetResourceGroups().Get(TestEnvironment.ResourceGroup).Value;
            }
        }

        public Resources.ResourceGroupResource GetResourceGroup(string name)
        {
            return Subscription.GetResourceGroups().Get(name).Value;
        }

        protected void Initialize()
        {
            ArmClient = GetArmClient();
        }

        protected async Task<ResourceGroupResource> CreateResourceGroup(string name)
        {
            return (await Subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, name, new ResourceGroupData(TestEnvironment.Location))).Value;
        }

        protected async Task<ResourceGroupResource> CreateResourceGroup(string name, string location)
        {
            return (await Subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, name, new ResourceGroupData(location))).Value;
        }

        protected async Task<GrafanaResource> CreateDefaultGrafanaResource(string grafanaResourcebName, AzureLocation location, ResourceGroupResource resourceGroup)
        {
            // Create grafanaResource
            var grafanaResource = await (await resourceGroup.GetGrafanaResources().CreateOrUpdateAsync(WaitUntil.Completed, grafanaResourcebName, null)).WaitForCompletionAsync();

            return grafanaResource.Value;
        }
    }
}
