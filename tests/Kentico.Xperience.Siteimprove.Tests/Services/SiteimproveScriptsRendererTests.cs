using Kentico.Web.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveScriptsRenderer"/> class.
    /// </summary>
    public class SiteimproveScriptsRendererTests
    {
        [TestFixture]
        public class RenderPluginScriptTagTests : SiteimproveScriptsRendererTestsBase
        {
            [Test]
            public void RenderPluginScriptTag_Valid_ReturnsCorrectScriptTag()
            {
                var scriptTag = scriptsRenderer.RenderPluginScriptTag();
                string result = GetValue(scriptTag);

                string expectedResult = $"<script async=\"\" src=\"{SiteimproveConstants.PLUGIN_URL}\" type=\"text/javascript\"></script>";

                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }


        [TestFixture]
        public class RenderConfigurationScriptTagTests : SiteimproveScriptsRendererTestsBase
        {
            private const string ROUTE = "http://localhost/admin/configurationscript.js";

            private IUrlHelper urlHelper;


            [SetUp]
            public void Setup()
            {
                actionContextAccessor.ActionContext = Substitute.For<ActionContext>(Substitute.For<HttpContext>(), new RouteData(), new ActionDescriptor());

                urlHelper = Substitute.For<IUrlHelper>();

                urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext).Returns(urlHelper);
            }


            [Test]
            public void RenderConfigurationScriptTag_Valid_ReturnsCorrectScriptTag()
            {
                int pageID = 20;
                string scriptUrl = $"{ROUTE}?pageId={pageID}";
                urlHelper.Kentico().RouteUrl(default, default, default, default).ReturnsForAnyArgs(scriptUrl);

                var scriptTag = scriptsRenderer.RenderConfigurationScriptTag(pageID);
                string result = GetValue(scriptTag);

                string expectedResult = $"<script async=\"\" src=\"{scriptUrl}\" type=\"text/javascript\"></script>";

                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
    }
}
