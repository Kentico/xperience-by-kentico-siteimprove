using CMS.Base.Internal;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Tests;

using Kentico.Content.Web.Mvc;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimprovePluginTagHelper"/> class.
    /// </summary>
    public class SiteimprovePluginTagHelperTests
    {
        [TestFixture]
        public class ProcessTests : UnitTests
        {
            private SiteimprovePluginTagHelper tagHelper;
            private TagHelperContext context;
            private TagHelperOutput output;
            private IPageDataContextRetriever pageDataContextRetriever;
            private IHttpContextRetriever httpContextRetriever;
            private IRequest httpRequest;
            private ISiteimproveScriptsRenderer scriptsRenderer;


            protected override void RegisterTestServices()
            {
                base.RegisterTestServices();

                pageDataContextRetriever = Substitute.For<IPageDataContextRetriever>();
                Service.Use<IPageDataContextRetriever>(pageDataContextRetriever);

                httpContextRetriever = Substitute.For<IHttpContextRetriever>();
                Service.Use<IHttpContextRetriever>(httpContextRetriever);

                scriptsRenderer = Substitute.For<ISiteimproveScriptsRenderer>();
                Service.Use<ISiteimproveScriptsRenderer>(scriptsRenderer);
            }


            [SetUp]
            public void SetUp()
            {
                string tagName = "siteimprove-plugin";
                tagHelper = new SiteimprovePluginTagHelper();
                context = new TagHelperContext(tagName, new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
                output = new TagHelperOutput(tagName, new TagHelperAttributeList(), (useCached, htmlEncoder) =>
                {
                    return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
                });

                var httpContext = Substitute.For<IHttpContext>();
                httpRequest = Substitute.For<IRequest>();
                httpRequest.UrlReferrer.Returns(new Uri("http://localhost/admin/pages/en-US_20/preview"));

                httpContext.Request.Returns(httpRequest);
                httpContextRetriever.GetContext().Returns(httpContext);

                VirtualContext.CurrentURLPrefix = "cmsctx";
            }


            [Test]
            public void Process_InvalidContext_ThrowsArgumentNullException()
            {
                Assert.That(() => tagHelper.Process(null, output), Throws.TypeOf<ArgumentNullException>());
            }


            [Test]
            public void Process_InvalidOutput_ThrowsArgumentNullException()
            {
                Assert.That(() => tagHelper.Process(context, null), Throws.TypeOf<ArgumentNullException>());
            }


            [Test]
            public void Process_VirtualContextNotInitialized_ClearsTagNameDoesNotModifyContent()
            {
                VirtualContext.CurrentURLPrefix = string.Empty;

                tagHelper.Process(context, output);

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.False);
                    Assert.That(scriptsRenderer.ReceivedCalls(), Is.Empty);

                    pageDataContextRetriever.DidNotReceive().TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }


            [TestCase("http://localhost/admin/pages/en-US_20/page-builder", TestName = "Process_URLReferrerInvalidPageBuilder_ClearsTagNameDoesNotModifyContent")]
            [TestCase("http://localhost", TestName = "Process_URLReferrerInvalidHome_ClearsTagNameDoesNotModifyContent")]
            [TestCase("http://localhost/", TestName = "Process_URLReferrerInvalidHomeEndSlash_ClearsTagNameDoesNotModifyContent")]
            [TestCase("http://localhost/about-us", TestName = "Process_URLReferrerInvalidAboutUs_ClearsTagNameDoesNotModifyContent")]
            public void Process_URLReferrerInvalid_ClearsTagNameDoesNotModifyContent(string url)
            {
                httpRequest.UrlReferrer.Returns(new Uri(url));

                tagHelper.Process(context, output);

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.False);
                    Assert.That(scriptsRenderer.ReceivedCalls(), Is.Empty);

                    pageDataContextRetriever.DidNotReceive().TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }


            [Test]
            public void Process_RetrieveFail_ClearsTagNameDoesNotModifyContent()
            {
                pageDataContextRetriever.TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>()).Returns(false);

                tagHelper.Process(context, output);

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.False);

                    Assert.That(scriptsRenderer.ReceivedCalls(), Is.Empty);

                    pageDataContextRetriever.Received(1).TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }



            [Test]
            public void Process_RetrieveSuccessInvalidPage_ClearsTagNameDoesNotModifyContent()
            {
                var pageDataContext = Substitute.For<IPageDataContext<TreeNode>>();
                pageDataContextRetriever.TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>()).Returns(x =>
                {
                    x[0] = pageDataContext;
                    return true;
                });

                tagHelper.Process(context, output);

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.False);
                    Assert.That(scriptsRenderer.ReceivedCalls(), Is.Empty);

                    pageDataContextRetriever.Received(1).TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }


            [Test]
            public void Process_RetrieveSuccess_ClearsTagNameOutputsCorrectTag()
            {
                int documentID = 20;
                var pageDataContext = Substitute.For<IPageDataContext<TreeNode>>();
                var node = Substitute.For<TreeNode>();
                node.DocumentID.Returns(documentID);
                pageDataContext.Page.Returns(node);
                pageDataContextRetriever.TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>()).Returns(x =>
                {
                    x[0] = pageDataContext;
                    return true;
                });

                string pluginScriptTag = "<script src=plugin></script>";
                string configurationScriptTag = "<script src=configuration></script>";

                scriptsRenderer.RenderPluginScriptTag().Returns(new HtmlString(pluginScriptTag));
                scriptsRenderer.RenderConfigurationScriptTag(documentID).Returns(new HtmlString(configurationScriptTag));

                tagHelper.Process(context, output);

                string result = output.Content.GetContent();
                string expectedResult = pluginScriptTag + Environment.NewLine + configurationScriptTag + Environment.NewLine;

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.True);
                    Assert.That(result, Is.EqualTo(expectedResult));

                    pageDataContextRetriever.Received(1).TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());

                    scriptsRenderer.Received(1).RenderPluginScriptTag();
                    scriptsRenderer.Received(1).RenderConfigurationScriptTag(documentID);
                });
            }
        }
    }
}
