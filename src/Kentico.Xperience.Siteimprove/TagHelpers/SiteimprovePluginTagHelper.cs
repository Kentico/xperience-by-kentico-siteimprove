using System.Text.RegularExpressions;
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
        private readonly IPageDataContextRetriever pageDataContextRetriever;
        private readonly ISiteimproveScriptsRenderer siteimproveScriptsRenderer;


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimprovePluginTagHelper"/> class.
        /// </summary>
        /// <param name="pageDataContextRetriever">Page data context retriever.</param>
        /// <param name="siteimproveScriptsRenderer">Siteimprove scripts renderer.</param>
        public SiteimprovePluginTagHelper(IPageDataContextRetriever pageDataContextRetriever, ISiteimproveScriptsRenderer siteimproveScriptsRenderer)
        {
            this.pageDataContextRetriever = pageDataContextRetriever;
            this.siteimproveScriptsRenderer = siteimproveScriptsRenderer;
        }


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

            pageDataContextRetriever.TryRetrieve<TreeNode>(out var data);

            var node = data?.Page;

            if (node == null)
            {
                return;
            }

            output.Content.SetHtmlContent(new HtmlContentBuilder()
                .AppendLine(siteimproveScriptsRenderer.RenderPluginScriptTag())
                .AppendLine(siteimproveScriptsRenderer.RenderConfigurationScriptTag(node.DocumentID)));
        }
    }
}
