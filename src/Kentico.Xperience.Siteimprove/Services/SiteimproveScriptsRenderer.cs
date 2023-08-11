using System.Web;

using CMS.Core;

using Kentico.Web.Mvc;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Kentico.Xperience.Siteimprove
{
    internal class SiteimproveScriptsRenderer : ISiteimproveScriptsRenderer
    {
        /// <inheritdoc/>
        public IHtmlContent RenderPluginScriptTag()
        {
            return RenderScriptTag(SiteimproveConstants.PLUGIN_URL);
        }


        /// <inheritdoc/>
        public IHtmlContent RenderConfigurationScriptTag(int pageId)
        {
            var actionContextAccessor = Service.Resolve<IActionContextAccessor>();
            var urlHelperFactory = Service.Resolve<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            var routeValues = new { pageId };

            string scriptUrl = urlHelper.Kentico().RouteUrl(SiteimproveConstants.CONFIGURATION_SCRIPT_ROUTE_NAME, null, null, routeValues);

            return RenderScriptTag(scriptUrl);
        }


        private IHtmlContent RenderScriptTag(string scriptUrl)
        {
            string scriptSrc = HttpUtility.HtmlAttributeEncode(HttpUtility.UrlPathEncode(scriptUrl));

            var script = new TagBuilder("script");
            script.Attributes.Add("src", scriptSrc);
            script.Attributes.Add("async", string.Empty);
            script.Attributes.Add("type", "text/javascript");

            return script;
        }
    }
}
