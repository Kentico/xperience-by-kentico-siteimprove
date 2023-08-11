using CMS;
using CMS.Core;

using Kentico.Xperience.Siteimprove;

[assembly: RegisterImplementation(typeof(ISiteimproveScriptsProvider), typeof(SiteimproveScriptsProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Interface for providing Siteimprove configuration script.
    /// </summary>
    internal interface ISiteimproveScriptsProvider
    {
        /// <summary>
        /// Returns configuration script of the Siteimprove plugin.
        /// </summary>
        /// <param name="pageId">ID of page to input to the plugin.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        Task<string> GetConfigurationScript(int pageId, CancellationToken? cancellationToken = null);
    }
}
