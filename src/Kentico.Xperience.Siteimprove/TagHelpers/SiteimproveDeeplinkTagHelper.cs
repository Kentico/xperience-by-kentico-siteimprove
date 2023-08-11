using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Content.Web.Mvc;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Tag Helper for adding meta tag for easier CMS Deeplink configuration.
    /// </summary>
    public class SiteimproveDeeplinkTagHelper : TagHelper
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

            var pageDataContextRetriever = Service.Resolve<IPageDataContextRetriever>();

            pageDataContextRetriever.TryRetrieve<TreeNode>(out var data);

            var node = data?.Page;

            if (node == null)
            {
                return;
            }

            string meta = $"<meta name=\"pageID\" content=\"{node.DocumentCulture}_{node.DocumentID}\" />";

            output.Content.SetHtmlContent(new HtmlContentBuilder()
                .AppendHtml(meta));
        }
    }
}
