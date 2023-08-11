using System.Net;
using System.Net.Http.Json;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Handles requests to Siteimprove.
    /// </summary>
    internal class SiteimproveService : ISiteimproveService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly SiteimproveOptions siteimproveOptions;
        private readonly IEventLogService eventLogService;

        private long siteID = -1;
        private string token = string.Empty;
        private bool isContentCheckEnabled = false;


        private HttpClient HttpClient => httpClientFactory.CreateClient(SiteimproveConstants.CLIENT_NAME);


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimproveService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Http client factory.</param>
        /// <param name="siteimproveOptions">Siteimprove options.</param>
        /// <param name="eventLogService">Event log service.</param>
        public SiteimproveService(
            IHttpClientFactory httpClientFactory,
            IOptions<SiteimproveOptions> siteimproveOptions,
            IEventLogService eventLogService)
        {
            this.httpClientFactory = httpClientFactory;
            this.siteimproveOptions = siteimproveOptions.Value;
            this.eventLogService = eventLogService;
        }


        /// <inheritdoc/>
        public async Task<string> GetToken(CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = await GetTokenInternal(cancellationToken);
            }

            return token;
        }


        /// <inheritdoc/>
        public async Task CheckPages(IEnumerable<string> urls, CancellationToken? cancellationToken = null)
        {
            var client = HttpClient;

            foreach (var url in urls)
            {
                string requestPath = string.Format(SiteimproveConstants.PAGE_CHECK_PATH, siteID, url);
                var response = await Post(requestPath, cancellationToken, client);

                if (response != null && !response.IsSuccessStatusCode)
                {
                    eventLogService.LogError(nameof(SiteimproveService), nameof(CheckPages), $"Failed to request check of a page '{url}'.");
                }
            }
        }


        /// <inheritdoc/>
        public Task<bool> IsContentCheckEnabled(CancellationToken? cancellationToken = null)
        {
            return Task.FromResult(isContentCheckEnabled);
        }


        /// <inheritdoc/>
        public async Task Initialize(CancellationToken? cancellationToken = null)
        {
            isContentCheckEnabled = await IsContentCheckEnabledInternal(cancellationToken);
            siteID = await GetSiteID(cancellationToken);
        }


        internal async Task<long> GetSiteID(CancellationToken? cancellationToken = null)
        {
            var client = HttpClient;
            string requestPath = string.Format(SiteimproveConstants.SITES_PATH, 1);
            var sites = await Get<SiteimproveSites>(requestPath, cancellationToken, client);

            if (sites == null)
            {
                return siteID;
            }

            requestPath = string.Format(SiteimproveConstants.SITES_PATH, sites.TotalItems);

            sites = await Get<SiteimproveSites>(requestPath, cancellationToken, client);

            if (sites == null)
            {
                return siteID;
            }

            string domain = GetDomain();

            var site = sites.Sites.FirstOrDefault(s => s.SiteUrl.Contains($"://{domain}"));

            if (site == null)
            {
                eventLogService.LogError(nameof(SiteimproveService), nameof(GetSiteID), $"Site with URL '{domain}' could not be found on Siteimprove.");
                return siteID;
            }

            return site.SiteID;
        }


        internal async Task<bool> IsContentCheckEnabledInternal(CancellationToken? cancellationToken = null)
        {
            if (!siteimproveOptions.EnableContentCheck)
            {
                return false;
            }

            var client = HttpClient;
            bool isContentCheckEnabled = await IsContentCheckReady(client, cancellationToken);

            if (!isContentCheckEnabled)
            {
                if (await TryEnableContentCheck(client, cancellationToken))
                {
                    isContentCheckEnabled = await IsContentCheckReady(client, cancellationToken);
                }
            }

            return isContentCheckEnabled;
        }


        private string GetDomain()
        {
            return RequestContext.CurrentDomain;
        }


        private async Task<bool> TryEnableContentCheck(HttpClient client, CancellationToken? cancellationToken = null)
        {
            var response = await Post(SiteimproveConstants.CONTENT_CHECK_PATH, cancellationToken, client);

            if (response == null)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                string message = response.StatusCode == HttpStatusCode.PaymentRequired
                    ? "Cannot enable content check. User not subscribed to Prepublish feature."
                    : "An error occurred during enabling of content check.";

                eventLogService.LogError(nameof(SiteimproveService), nameof(TryEnableContentCheck), message);

                return false;
            }

            return true;
        }


        private async Task<bool> IsContentCheckReady(HttpClient client, CancellationToken? cancellationToken = null)
        {
            var contentCheck = await Get<SiteimproveContentCheck>(SiteimproveConstants.CONTENT_CHECK_PATH, cancellationToken, client);

            if (contentCheck == null)
            {
                return false;
            }

            return contentCheck.IsReady;
        }


        private async Task<string> GetTokenInternal(CancellationToken? cancellationToken = null)
        {
            string token = SettingsKeyInfoProvider.GetValue(SiteimproveConstants.TOKEN_SETTINGS_KEY_NAME);

            if (string.IsNullOrEmpty(token))
            {
                string requestPath = string.Format(SiteimproveConstants.TOKEN_URL, Uri.EscapeDataString(SiteimproveConstants.CMS_NAME));
                var receivedToken = await Get<SiteimproveToken>(requestPath, cancellationToken);

                if (receivedToken == null)
                {
                    return token;
                }

                token = receivedToken.Value;
                SetTokenSettings(token);
            }

            return token;
        }


        private async Task<T> Get<T>(string requestPath, CancellationToken? cancellationToken = null, HttpClient client = null) where T : class
        {
            client ??= HttpClient;

            try
            {
                return await client.GetFromJsonAsync<T>(requestPath, cancellationToken: cancellationToken ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SiteimproveService), nameof(Get), ex);
            }

            return null;
        }


        private async Task<HttpResponseMessage> Post(string requestPath, CancellationToken? cancellationToken = null, HttpClient client = null)
        {
            client ??= HttpClient;

            try
            {
                return await client.PostAsync(requestPath, null, cancellationToken ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SiteimproveService), nameof(Get), ex);
            }

            return null;
        }


        private void SetTokenSettings(string token)
        {
            var settingsInfo = new SettingsKeyInfo()
            {
                KeyName = SiteimproveConstants.TOKEN_SETTINGS_KEY_NAME,
                KeyDisplayName = SiteimproveConstants.TOKEN_SETTINGS_KEY_DISPLAY_NAME,
                KeyDescription = SiteimproveConstants.TOKEN_SETTINGS_KEY_DESCRIPTION,
                KeyValue = token,
                KeyType = "string",
                KeyCategoryID = GetCategoryID(),
                KeyGUID = Guid.NewGuid(),
                KeyLastModified = DateTime.UtcNow,
                KeyIsHidden = true,
                KeyExplanationText = string.Empty,
                KeyIsCustom = true
            };

            SettingsKeyInfoProvider.SetSettingsKeyInfo(settingsInfo);
        }


        private int GetCategoryID()
        {
            int rootCategoryID = SettingsCategoryInfoProvider.RootCategory.CategoryID;
            var rootChildCategories = SettingsCategoryInfoProvider.GetChildSettingsCategories(rootCategoryID);
            var integrationsCategory = rootChildCategories.Items.
                Where(c => string.Equals(c.CategoryName, SiteimproveConstants.CATEGORY_NAME, StringComparison.Ordinal)).FirstOrDefault();

            return integrationsCategory != null ? integrationsCategory.CategoryID : rootCategoryID;
        }
    }
}
