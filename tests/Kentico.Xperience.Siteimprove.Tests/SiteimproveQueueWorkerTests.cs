using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using CMS.SiteProvider;
using CMS.Tests;

using NSubstitute;

using NUnit.Framework;

using Tests.DocumentEngine;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveQueueWorker"/> class.
    /// </summary>
    public class SiteimproveQueueWorkerTests
    {
        [TestFixture]
        public class EnqueueNodesTests : UnitTests
        {
            private const string CLASS_NAME = "testClassUrl";
            private const string DOMAIN = "test.com";
            private const string ALIAS_1 = "test1";
            private const string ALIAS_2 = "test2";
            private const string ALIAS_3 = "test3";
            private const int SITE_ID = 1;

            private ISiteimproveService service;
            private int nodeID;


            protected override void RegisterTestServices()
            {
                base.RegisterTestServices();

                service = Substitute.For<ISiteimproveService>();
                Service.Use<ISiteimproveService>(service);
            }


            [SetUp]
            public void Setup()
            {
                nodeID = 0;

                Fake<DataClassInfo, DataClassInfoProvider>().WithData(DataClassInfo.New(c =>
                {
                    c.ClassID = 1;
                    c.ClassGUID = new Guid("00000000-0000-0000-0000-000000000001");
                    c.ClassName = CLASS_NAME;
                    c.ClassIsDocumentType = true;
                    c.ClassIsCoupledClass = true;
                    c.ClassURLPattern = "/{%DocumentCulture%}/{%NodeAliasPath%}";
                    c.ClassHasURL = true;
                }));

                DocumentGenerator.RegisterDocumentType<TreeNode>(CLASS_NAME);
                Fake().DocumentType<TreeNode>(CLASS_NAME);

                Fake<SiteInfo, SiteInfoProvider>().WithData(new SiteInfo
                {
                    SiteID = SITE_ID,
                    SiteName = "test",
                    SiteDomainName = DOMAIN
                });

                Fake<SiteDomainAliasInfo, SiteDomainAliasInfoProvider>().WithData();

                Fake<SettingsKeyInfo, SettingsKeyInfoProvider>().WithData(new SettingsKeyInfo
                {
                    KeyName = PageRoutingHelper.ROUTING_MODE_KEY,
                    KeyType = "int",
                    KeyValue = ((int)PageRoutingModeEnum.Custom).ToString()
                });

            }


            [TearDown]
            public void TearDown()
            {
                ResetAllFakes();
            }


            [Test]
            public void EnqueueNodes_AllNodesPublicUrl_PageChecksRequested()
            {
                var nodes = new List<TreeNode>
                {
                    CreateMockNode(ALIAS_1),
                    CreateMockNode(ALIAS_2),
                    CreateMockNode(ALIAS_3),
                };

                SiteimproveQueueWorker.EnqueueNodes(nodes);

                Assert.Multiple(async () =>
                {
                    await service.Received(3).CheckPages(Arg.Any<IEnumerable<string>>());
                    await service.Received(1).CheckPages(Arg.Is<IEnumerable<string>>(e => e.First().Contains(ALIAS_1)));
                    await service.Received(1).CheckPages(Arg.Is<IEnumerable<string>>(e => e.First().Contains(ALIAS_2)));
                    await service.Received(1).CheckPages(Arg.Is<IEnumerable<string>>(e => e.First().Contains(ALIAS_3)));
                });
            }


            [Test]
            public void EnqueueNodes_OneNodePublicUrl_PageRecheckRequested()
            {
                var nodes = new List<TreeNode>
                {
                    CreateMockNode(ALIAS_1, isSecured : true),
                    CreateMockNode(ALIAS_2),
                    CreateMockNode(ALIAS_3, isSecured: true),
                };

                SiteimproveQueueWorker.EnqueueNodes(nodes);

                Assert.Multiple(async () =>
                {
                    await service.Received(1).CheckPages(Arg.Any<IEnumerable<string>>());
                    await service.Received(1).CheckPages(Arg.Is<IEnumerable<string>>(e => e.First().Contains(ALIAS_2)));
                });
            }


            [Test]
            public void EnqeueNodes_AllNodesWithoutUrl_PageRechecksNotRequested()
            {
                ResetAllFakes();
                string className = $"{CLASS_NAME}_NO_URL";
                Fake<DataClassInfo, DataClassInfoProvider>().WithData(DataClassInfo.New(c =>
                {
                    c.ClassID = 2;
                    c.ClassGUID = new Guid("00000000-0000-0000-0000-000000000002");
                    c.ClassName = className;
                    c.ClassIsDocumentType = true;
                    c.ClassIsCoupledClass = true;
                    c.ClassHasURL = false;
                }));

                DocumentGenerator.RegisterDocumentType<TreeNode>(className);
                Fake().DocumentType<TreeNode>(className);

                var nodes = new List<TreeNode>
                {
                    CreateMockNode(ALIAS_1, className),
                    CreateMockNode(ALIAS_2, className),
                    CreateMockNode(ALIAS_3, className),
                };

                SiteimproveQueueWorker.EnqueueNodes(nodes);

                Assert.Multiple(async () =>
                {
                    await service.Received(0).CheckPages(Arg.Any<IEnumerable<string>>());
                });
            }


            private TreeNode CreateMockNode(string aliasPath, string className = CLASS_NAME, bool isSecured = false)
            {
                int id = nodeID++;
                var node = TreeNode.New(className).With(
                    t =>
                    {
                        t.SetValue("NodeID", id);
                        t.SetValue("NodeSiteID", SITE_ID);
                        t.SetValue("NodeClassID", 1);
                        t.SetValue("NodeAliasPath", aliasPath);
                        t.SetValue("DocumentCulture", "en-US");
                        t.SetValue("NodeIsSecured", isSecured);
                        t.DocumentName = $"Document {id}";
                    }
                );

                return node;
            }
        }
    }
}
