using System.Text.Json.Serialization;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents single site.
    /// </summary>
    internal class SiteimproveSite
    {
        /// <summary>
        /// Site ID.
        /// </summary>
        [JsonPropertyName("id")]
        public long SiteID { get; set; }


        /// <summary>
        /// Site domain URL.
        /// </summary>
        [JsonPropertyName("url")]
        public string SiteUrl { get; set; }
    }
}
