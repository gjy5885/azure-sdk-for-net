// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Core.TestFramework;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Dashboard.Models;
using Azure.ResourceManager.Dashboard.Tests.Helpers;
using NUnit.Framework;
using Azure.Core;
using Azure.ResourceManager.Dashboard;

namespace Azure.ResourceManager.Dashboard.Tests
{
    public class GrafanaResourceTests : GrafanaResourceServiceClientTestBase
    {
        private ResourceGroupResource _resourceGroup;

        private ResourceIdentifier _resourceGroupIdentifier;

        public GrafanaResourceTests(bool isAsync) : base(isAsync, RecordedTestMode.Record)
        {
        }

        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            var rgLro = await GlobalClient.GetDefaultSubscriptionAsync().Result.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, SessionRecording.GenerateAssetName("GrafanaResourceRG-"), new ResourceGroupData(AzureLocation.WestUS2));
            ResourceGroupResource rg = rgLro.Value;
            _resourceGroupIdentifier = rg.Id;
            await StopSessionRecordingAsync();
        }

        [OneTimeTearDown]
        public async Task GlobleTearDown()
        {
            await _resourceGroup.DeleteAsync(WaitUntil.Completed);
        }

        [SetUp]
        public async Task TestSetUp()
        {
            var client = GetArmClient();
            _resourceGroup = await client.GetResourceGroupResource(_resourceGroupIdentifier).GetAsync();
        }

        [TearDown]
        public async Task TestTearDown()
        {
            var list = await _resourceGroup.GetGrafanaResources().GetAllAsync().ToEnumerableAsync();
            foreach (var item in list)
            {
                await item.DeleteAsync(WaitUntil.Completed);
            }
        }

        public async Task<GrafanaResource> CreateGrafanaResource(string grafanaResourceName)
        {
            /*            // Create WebPubSub ConfigData
                        IList<LiveTraceCategory> categories = new List<LiveTraceCategory>();
                        categories.Add(new LiveTraceCategory("category-01", "true"));

                        AclAction aclAction = new AclAction("Deny");
                        IList<WebPubSubRequestType> allow = new List<WebPubSubRequestType>();
                        IList<WebPubSubRequestType> deny = new List<WebPubSubRequestType>();
                        //allow.Add(new WebPubSubRequestType("ClientConnectionValue"));
                        deny.Add(new WebPubSubRequestType("RESTAPI"));
                        NetworkAcl publicNetwork = new NetworkAcl(allow, deny);
                        IList<PrivateEndpointAcl> privateEndpoints = new List<PrivateEndpointAcl>();

                        List<ResourceLogCategory> resourceLogCategory = new List<ResourceLogCategory>()
                        {
                            new ResourceLogCategory(){ Name = "category1", Enabled = "false" }
                        };*/
            var identityType = new IdentityType("SystemAssigned");
            GrafanaResourceData data = new GrafanaResourceData(AzureLocation.WestUS2)
            {
                Sku = new ResourceSku("Standard"),
                Identity = new ManagedIdentity(),
                Properties = new GrafanaResourceProperties(),
            };
            // Create GrafanaResource
            var grafanaResource = await (await _resourceGroup.GetGrafanaResources().CreateOrUpdateAsync(WaitUntil.Completed, grafanaResourceName, data)).WaitForCompletionAsync();

            return grafanaResource.Value;
        }

        [Test]
        [RecordedTest]
        public async Task CreateOrUpdate()
        {
            string grafanaResourceName = Recording.GenerateAssetName("grafanaResource-");
            var grafanaResource = await CreateGrafanaResource(grafanaResourceName);
            Console.WriteLine(grafanaResource.Data.Name);
            Assert.IsNotNull(grafanaResource.Data);
            Assert.AreEqual(grafanaResourceName, grafanaResource.Data.Name);
            Assert.AreEqual(AzureLocation.WestUS2, grafanaResource.Data.Location);
        }

        [Test]
        [RecordedTest]
        public async Task CheckIfExist()
        {
            string grafanaResourceName = Recording.GenerateAssetName("grafanaResource-");
            await CreateGrafanaResource(grafanaResourceName);
            Assert.IsTrue(await _resourceGroup.GetGrafanaResources().ExistsAsync(grafanaResourceName));
            Assert.IsFalse(await _resourceGroup.GetGrafanaResources().ExistsAsync(grafanaResourceName + "1"));
        }

        [Test]
        [RecordedTest]
        public async Task Get()
        {
            string grafanaResourceName = Recording.GenerateAssetName("grafanaResource-");
            await CreateGrafanaResource(grafanaResourceName);
            var grafanaResource = await _resourceGroup.GetGrafanaResources().GetAsync(grafanaResourceName);
            Assert.IsNotNull(grafanaResource.Value.Data);
            Assert.AreEqual(grafanaResourceName, grafanaResource.Value.Data.Name);
            Assert.AreEqual(AzureLocation.WestUS2, grafanaResource.Value.Data.Location);
        }

        [Test]
        [RecordedTest]
        public async Task GetAll()
        {
            string grafanaResourceName = Recording.GenerateAssetName("grafanaResource-");
            await CreateGrafanaResource(grafanaResourceName);
            List<GrafanaResource> grafanaResourceList = await _resourceGroup.GetGrafanaResources().GetAllAsync().ToEnumerableAsync();
            Assert.AreEqual(1, grafanaResourceList.Count);
        }

        [Test]
        [RecordedTest]
        public async Task Delete()
        {
            string grafanaResourceName = Recording.GenerateAssetName("grafanaResource-");
            var grafanaResource = await CreateGrafanaResource(grafanaResourceName);
            await grafanaResource.DeleteAsync(WaitUntil.Completed);
        }
    }
}
