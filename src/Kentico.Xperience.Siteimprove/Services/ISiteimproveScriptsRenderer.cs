using CMS;
using CMS.Core;

using Kentico.Xperience.Siteimprove;

using Microsoft.AspNetCore.Html;

[assembly: RegisterImplementation(typeof(ISiteimproveScriptsRenderer), typeof(SiteimproveScriptsRenderer), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Interface for rendering Siteimprove script tags.
    /// </summary>
    internal interface ISiteimproveScriptsRenderer
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
