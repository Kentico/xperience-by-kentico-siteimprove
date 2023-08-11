namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Constants used by Siteimprove integration.
    /// </summary>
    public static class SiteimproveConstants
    {
        /// <summary>
        /// Base URL of Siteimprove API.
        /// </summary>
        public const string BASE_URL = "https://api.siteimprove.com/v2/";


        /// <summary>
        /// Name of the integration settings category.
        /// </summary>
        public const string CATEGORY_NAME = "CMS.Integration";


        /// <summary>
        /// Name of the named HTTP client.
        /// </summary>
        public const string CLIENT_NAME = "SiteimproveClient";


        /// <summary>
        /// Name of the CMS.
        /// </summary>
        public const string CMS_NAME = "Xperience by Kentico";


        /// <summary>
        /// Name of configuration script route.
        /// </summary>
        public const string CONFIGURATION_SCRIPT_ROUTE_NAME = "SiteimproveConfigurationScript";


        /// <summary>
        /// Pattern of the configuration script route.
        /// </summary>
        public const string CONFIGURATION_SCRIPT_ROUTE_PATTERN = "siteimprove/configurationscript.js/{pageId?}";


        /// <summary>
        /// Path for Prepublish check requests.
        /// </summary>
        public const string CONTENT_CHECK_PATH = "settings/content_checking";


        /// <summary>
        /// Path for page check requests.
        /// </summary>
        public const string PAGE_CHECK_PATH = "sites/{0}/content/check/page?url={1}";


        /// <summary>
        /// URL of the Siteimprove plugin.
        /// </summary>
        public const string PLUGIN_URL = "https://cdn.siteimprove.net/cms/overlay-v2.js";


        /// <summary>
        /// Referrer pattern of the Preview tab.
        /// </summary>
        public const string REFERRER_PATTERN = @"/admin/pages/.*/preview";


        /// <summary>
        /// Name of the Siteimprove section in the 'appsettings.json' file.
        /// </summary>
        public const string SECTION_KEY = "Siteimprove";


        /// <summary>
        /// Path for retrieving sites.
        /// </summary>
        public const string SITES_PATH = "sites?page=1&page_size={0}";


        /// <summary>
        /// Name of the token settings key. 
        /// </summary>
        public const string TOKEN_SETTINGS_KEY_NAME = "SiteimproveToken";


        /// <summary>
        /// Display name of the token settings key.
        /// </summary>
        public const string TOKEN_SETTINGS_KEY_DISPLAY_NAME = "{$siteimprove.settings.token$}";


        /// <summary>
        /// Description of the token settings key.
        /// </summary>
        public const string TOKEN_SETTINGS_KEY_DESCRIPTION = "{$siteimprove.settings.token.description$}";


        /// <summary>
        /// URL for retrieving token.
        /// </summary>
        public const string TOKEN_URL = "https://my2.siteimprove.com/auth/token?cms={0}";
    }
}
