using CMS.DocumentEngine;

using Kentico.Content.Web.Mvc.Internal;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Provides Siteimprove configuration script.
    /// </summary>
    internal class SiteimproveScriptsProvider : ISiteimproveScriptsProvider
    {
        private readonly ISiteimproveService siteimproveService;
        private readonly IPageSystemDataContextRetriever pageDataRetriever;


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimproveScriptsProvider"/> class.
        /// </summary>
        /// <param name="siteimproveService">Siteimprove service.</param>
        /// <param name="pageDataRetriever">Page retriever.</param>
        public SiteimproveScriptsProvider(
            ISiteimproveService siteimproveService,
            IPageSystemDataContextRetriever pageDataRetriever)
        {
            this.siteimproveService = siteimproveService;
            this.pageDataRetriever = pageDataRetriever;
        }


        /// <inheritdoc/>
        public async Task<string> GetConfigurationScript(int pageId, CancellationToken? cancellationToken = null)
        {
            var node = pageDataRetriever.Retrieve(pageId, false, true);

            string token = await siteimproveService.GetToken(cancellationToken);
            string url = node != null ? $"'{DocumentURLProvider.GetAbsoluteUrl(node)}'" : "null";
            string contentCheckEnabled = await siteimproveService.IsContentCheckEnabled(cancellationToken) ? "true" : "false";

            return $"{Scripts.PluginConfiguration}('{token}', {url}, {contentCheckEnabled});";
        }
    }
}
