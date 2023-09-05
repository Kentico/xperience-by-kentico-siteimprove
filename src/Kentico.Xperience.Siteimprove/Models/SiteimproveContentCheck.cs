using System.Text.Json.Serialization;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents response of content check status request.
    /// </summary>
    internal class SiteimproveContentCheck
    {
        /// <summary>
        /// Indicates whether content check is ready for use.
        /// </summary>
        [JsonPropertyName("is_ready")]
        public bool IsReady { get; set; }
    }
}
