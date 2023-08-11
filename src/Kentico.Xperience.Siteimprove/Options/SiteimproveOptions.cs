namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents Siteimprove options.
    /// </summary>
    public sealed class SiteimproveOptions
    {
        /// <summary>
        /// API User used to send requests.
        /// </summary>
        public string APIUser { get; set; }


        /// <summary>
        /// API Key used to send requests.
        /// </summary>
        public string APIKey { get; set; }


        /// <summary>
        /// Indicates whether content check should be enabled on application startup or not.
        /// </summary>
        public bool EnableContentCheck { get; set; }
    }
}
