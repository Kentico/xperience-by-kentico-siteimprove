using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Startup extensions necessary for Siteimprove.
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Initializes <see cref="SiteimproveOptions"/>.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSiteimprove(this IServiceCollection services, IConfiguration configuration)
        {
            var siteimproveSection = configuration.GetSection(SiteimproveConstants.SECTION_KEY);
            services.Configure<SiteimproveOptions>(siteimproveSection);

            var siteimproveOptions = siteimproveSection.Get<SiteimproveOptions>();

            if (string.IsNullOrEmpty(siteimproveOptions.APIUser))
            {
                throw new InvalidOperationException(nameof(siteimproveOptions.APIUser));
            }

            if (string.IsNullOrEmpty(siteimproveOptions.APIKey))
            {
                throw new InvalidOperationException(nameof(siteimproveOptions.APIKey));
            }

            services.AddHttpClient(SiteimproveConstants.CLIENT_NAME, client =>
            {
                client.BaseAddress = new Uri(SiteimproveConstants.BASE_URL);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddDefaultAuthorization(client, siteimproveOptions);
            });

            return services;
        }


        /// <summary>
        /// Maps route of configuration script for Siteimprove plugin.
        /// </summary>
        /// <param name="app">Application builder.</param>
        public static IApplicationBuilder UseSiteimprove(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: SiteimproveConstants.CONFIGURATION_SCRIPT_ROUTE_NAME,
                    pattern: SiteimproveConstants.CONFIGURATION_SCRIPT_ROUTE_PATTERN,
                    defaults: new { controller = "Siteimprove", action = "ConfigurationScript" }
                );
            });

            return app;
        }


        private static void AddDefaultAuthorization(HttpClient httpClient, SiteimproveOptions siteimproveOptions)
        {
            var byteArray = Encoding.UTF8.GetBytes($"{siteimproveOptions.APIUser}:{siteimproveOptions.APIKey}");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
