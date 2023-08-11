using System.Text.Json.Serialization;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents response of the token retrieval request.
    /// </summary>
    internal class SiteimproveToken
    {
        /// <summary>
        /// Token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Value { get; set; }
    }
}
