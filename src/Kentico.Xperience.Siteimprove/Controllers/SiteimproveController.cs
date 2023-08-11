using CMS.Core;

using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Controller responsible for serving scripts necessary for configuring the Siteimprove plugin.
    /// </summary>
    /// <remarks>
    /// Requires the route to be mapped on startup via the <see cref="StartupExtensions.UseSiteimprove"/> method.
    /// </remarks>
    public class SiteimproveController : ControllerBase
    {
        private readonly ISiteimproveScriptsProvider siteimproveScriptsProvider;


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimproveController"/> class.
        /// </summary>
        public SiteimproveController()
        {
            siteimproveScriptsProvider = Service.Resolve<ISiteimproveScriptsProvider>();
        }


        /// <summary>
        /// Returns a JavaScript file which configures the Siteimprove plugin.
        /// </summary>
        /// <param name="pageId">ID of a page to input to the plugin.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        public async Task<IActionResult> ConfigurationScript(int pageId, CancellationToken cancellationToken)
        {
            return Content(await siteimproveScriptsProvider.GetConfigurationScript(pageId, cancellationToken), "application/javascript");
        }
    }
}
