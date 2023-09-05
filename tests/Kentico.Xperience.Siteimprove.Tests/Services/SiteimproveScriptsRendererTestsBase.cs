using System.Text.Encodings.Web;
using CMS.Tests;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Base class for <see cref="SiteimproveScriptsRenderer"/> tests.
    /// </summary>
    public class SiteimproveScriptsRendererTestsBase : UnitTests
    {
        private protected IActionContextAccessor actionContextAccessor;
        private protected IUrlHelperFactory urlHelperFactory;
        private protected ISiteimproveScriptsRenderer scriptsRenderer;


        [SetUp]
        public void SetUp()
        {
            actionContextAccessor = Substitute.For<IActionContextAccessor>();

            urlHelperFactory = Substitute.For<IUrlHelperFactory>();

            scriptsRenderer = new SiteimproveScriptsRenderer(actionContextAccessor, urlHelperFactory);
        }


        protected string GetValue(IHtmlContent htmlContent)
        {
            var writer = new StringWriter();
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
