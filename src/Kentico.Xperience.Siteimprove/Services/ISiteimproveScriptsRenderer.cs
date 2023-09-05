using Microsoft.AspNetCore.Html;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Interface for rendering Siteimprove script tags.
    /// </summary>
    public interface ISiteimproveScriptsRenderer
    {
        /// <summary>
        /// Renders script tag with src set to URL of Siteimprove plugin.
        /// </summary>
        IHtmlContent RenderPluginScriptTag();


        /// <summary>
        /// Renders script tag with src set to URL of the configuration of the Siteimprove plugin.
        /// </summary>
        /// <param name="pageId">ID of page to input to the plugin.</param>
        IHtmlContent RenderConfigurationScriptTag(int pageId);
    }
}
