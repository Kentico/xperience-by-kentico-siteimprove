using System.Text.Json.Serialization;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents response of the site listing request.
    /// </summary>
    internal class SiteimproveSites
    {
        /// <summary>
        /// Collection of sites.
        /// </summary>
        [JsonPropertyName("items")]
        public IEnumerable<SiteimproveSite> Sites { get; set; }


        /// <summary>
        /// Total number of sites.
        /// </summary>
        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }
    }
}
