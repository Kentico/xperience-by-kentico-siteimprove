using CMS.DocumentEngine;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents item queued for processing.
    /// </summary>
    internal class SiteimproveQueueItem
    {
        /// <summary>
        /// Node to process.
        /// </summary>
        public TreeNode Node { get; init; }
    }
}