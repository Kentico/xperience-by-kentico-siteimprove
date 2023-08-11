using CMS;
using CMS.Core;

using Kentico.Xperience.Siteimprove;

[assembly: RegisterImplementation(typeof(ISiteimproveService), typeof(SiteimproveService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Interface for handling requests to Siteimprove.
    /// </summary>
    internal interface ISiteimproveService
    {
        /// <summary>
        /// Returns plugin token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        Task<string> GetToken(CancellationToken? cancellationToken = null);


        /// <summary>
        /// Requests a check of provided <paramref name="urls"/>.
        /// </summary>
        /// <param name="urls">URLs to check.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        Task CheckPages(IEnumerable<string> urls, CancellationToken? cancellationToken = null);


        /// <summary>
        /// <see langword="true"/> if content check is ready and user is subscribed to Prepublish feature, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        Task<bool> IsContentCheckEnabled(CancellationToken? cancellationToken = null);


        /// <summary>
        /// Initializes the service. Used for asynchronous operations required to setup the service, such as sending requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <remarks>
        /// Method is called automatically on the first request during application startup in <see cref="SiteimproveModule"/>.
        /// </remarks>
        Task Initialize(CancellationToken? cancellationToken = null);
    }
}
