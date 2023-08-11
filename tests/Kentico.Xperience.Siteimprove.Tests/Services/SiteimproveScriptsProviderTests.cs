using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Tests;

using Kentico.Content.Web.Mvc.Internal;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveScriptsProvider"/> class.
    /// </summary>
    public class SiteimproveScriptsProviderTests
    {
        [TestFixture]
        public class GetConfigurationScriptTests : UnitTests
        {
            private const string TOKEN = "testToken";
            private const string DOMAIN = "test.com";

            private ISiteimproveScriptsProvider scriptsProvider;
            private ISiteimproveService service;
            private IPageSystemDataContextRetriever pageSystemDataContextRetriever;


            [SetUp]
            public void SetUp()
            {
                service = Substitute.For<ISiteimproveService>();
                pageSystemDataContextRetriever = Substitute.For<IPageSystemDataContextRetriever>();

                service.GetToken().Returns(Task.FromResult(TOKEN));

                scriptsProvider = new SiteimproveScriptsProvider(service, pageSystemDataContextRetriever);
            }


            [Test]
            public async Task GetConfigurationScript_NonExistingPage_ReturnsCorrectScriptWithNullUrl()
            {
                int pageId = 1;
                pageSystemDataContextRetriever.Retrieve(pageId, false, true).Returns((TreeNode)null);

                bool enableContentCheck = true;
                service.IsContentCheckEnabled().Returns(Task.FromResult(enableContentCheck));

                var result = await scriptsProvider.GetConfigurationScript(pageId);

                var expectedResult = $"{Scripts.PluginConfiguration}('{TOKEN}', null, {enableContentCheck.ToString().ToLower()});";

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(expectedResult));
                });
            }


            [TestCase(true, TestName = "GetConfigurationScript_ExistingPageContentCheckEnabled_ReturnsCorrectScriptWithCorrectUrl")]
            [TestCase(false, TestName = "GetConfigurationScript_ExistingPageContentCheckDisabled_ReturnsCorrectScriptWithCorrectUrl")]
            public async Task GetConfigurationScript_ExistingPage_ReturnsCorrectScriptWithCorrectUrl(bool enableContentCheck)
            {
                int siteId = 1;
                int pageId = 20;
                string documentCulture = "en-US";
                string nodeUrl = "test";

                Fake<SiteInfo, SiteInfoProvider>().WithData(new SiteInfo
                {
                    SiteID = siteId,
                    SiteName = "test",
                    SiteDomainName = DOMAIN
                });

                Fake<SiteDomainAliasInfo, SiteDomainAliasInfoProvider>().WithData();

                var fakeDocumentURLProvider = new FakeDocumentURLProvider(nodeUrl);
                DocumentURLProvider.ProviderObject = fakeDocumentURLProvider;

                var mockedTreeNode = Substitute.For<TreeNode>();
                mockedTreeNode.NodeSiteID.Returns(siteId);
                mockedTreeNode.DocumentCulture.Returns(documentCulture);
                pageSystemDataContextRetriever.Retrieve(pageId, false, true).Returns(mockedTreeNode);

                service.IsContentCheckEnabled().Returns(Task.FromResult(enableContentCheck));

                var result = await scriptsProvider.GetConfigurationScript(pageId);

                var expectedResult = $"{Scripts.PluginConfiguration}('{TOKEN}', 'http://{DOMAIN}/{nodeUrl}', {enableContentCheck.ToString().ToLower()});";

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(expectedResult));
                });
            }


            public class FakeDocumentURLProvider : DocumentURLProvider
            {
                private string url;


                public FakeDocumentURLProvider(string url)
                {
                    this.url = url;
                }


                protected override string GetUrlInternal(TreeNode node, string cultureCode)
                {
                    return url;
                }
            }
        }
    }
}
