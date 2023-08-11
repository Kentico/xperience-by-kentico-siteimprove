using System.Text.RegularExpressions;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Content.Web.Mvc;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Tag Helper for injecting Siteimprove plugin and plugin configuration scripts.
    /// </summary>
    public class SiteimprovePluginTagHelper : TagHelper
    {
        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.TagName = null;

            if (!VirtualContext.IsInitialized)
            {
                return;
            }

            if (!Regex.IsMatch(RequestContext.URLReferrer, SiteimproveConstants.REFERRER_PATTERN))
            {
                return;
            }

            var pageDataContextRetriever = Service.Resolve<IPageDataContextRetriever>();

            pageDataContextRetriever.TryRetrieve<TreeNode>(out var data);

            var node = data?.Page;

            if (node == null)
            {
                return;
            }

            var scriptsRenderer = Service.Resolve<ISiteimproveScriptsRenderer>();

            output.Content.SetHtmlContent(new HtmlContentBuilder()
                .AppendLine(scriptsRenderer.RenderPluginScriptTag())
                .AppendLine(scriptsRenderer.RenderConfigurationScriptTag(node.DocumentID)));
        }
    }
}
