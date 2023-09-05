using CMS.Core;
using CMS.DocumentEngine;
using CMS.Tests;

using Kentico.Content.Web.Mvc;

using Microsoft.AspNetCore.Razor.TagHelpers;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveDeeplinkTagHelper"/> class.
    /// </summary>
    public class SiteimproveDeeplinkTagHelperTests
    {
        [TestFixture]
        public class ProcessTests : UnitTests
        {
            private SiteimproveDeeplinkTagHelper tagHelper;
            private TagHelperContext context;
            private TagHelperOutput output;
            private IPageDataContextRetriever pageDataContextRetriever;


            protected override void RegisterTestServices()
            {
                base.RegisterTestServices();

                pageDataContextRetriever = Substitute.For<IPageDataContextRetriever>();
                Service.Use<IPageDataContextRetriever>(pageDataContextRetriever);
            }


            [SetUp]
            public void SetUp()
            {
                string tagName = "siteimprove-deeplink";
                tagHelper = new SiteimproveDeeplinkTagHelper();
                context = new TagHelperContext(tagName, new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
                output = new TagHelperOutput(tagName, new TagHelperAttributeList(), (useCached, htmlEncoder) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
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
            public void Process_RetrieveFail_ClearsTagNameDoesNotModifyContent()
            {
                pageDataContextRetriever.TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>()).Returns(false);

                tagHelper.Process(context, output);

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.False);

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

                    pageDataContextRetriever.Received(1).TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }



            [Test]
            public void Process_RetrieveSuccess_ClearsTagNameOutputsCorrectTag()
            {
                int documentID = 20;
                string documentCulture = "en-US";
                var pageDataContext = Substitute.For<IPageDataContext<TreeNode>>();
                var node = Substitute.For<TreeNode>();
                node.DocumentID.Returns(documentID);
                node.DocumentCulture.Returns(documentCulture);
                pageDataContext.Page.Returns(node);
                pageDataContextRetriever.TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>()).Returns(x =>
                {
                    x[0] = pageDataContext;
                    return true;
                });

                tagHelper.Process(context, output);

                string result = output.Content.GetContent();

                string expectedResult = $"<meta name=\"pageID\" content=\"{documentCulture}_{documentID}\" />";

                Assert.Multiple(() =>
                {
                    Assert.That(output.TagName, Is.Null);
                    Assert.That(output.IsContentModified, Is.True);
                    Assert.That(result, Is.EqualTo(expectedResult));

                    pageDataContextRetriever.Received(1).TryRetrieve(out Arg.Any<IPageDataContext<TreeNode>>());
                });
            }
        }
    }
}
