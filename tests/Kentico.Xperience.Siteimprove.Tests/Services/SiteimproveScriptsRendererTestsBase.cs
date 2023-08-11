using System.Text.Encodings.Web;

using CMS.Tests;

using Microsoft.AspNetCore.Html;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Base class for <see cref="SiteimproveScriptsRenderer"/> tests.
    /// </summary>
    public class SiteimproveScriptsRendererTestsBase : UnitTests
    {
        private protected ISiteimproveScriptsRenderer scriptsRenderer;


        [SetUp]
        public void SetUp()
        {
            scriptsRenderer = new SiteimproveScriptsRenderer();
        }


        protected string GetValue(IHtmlContent htmlContent)
        {
            var writer = new StringWriter();
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
