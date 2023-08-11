using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Background thread worker for processing Siteimprove URL checks.
    /// </summary>
    internal class SiteimproveQueueWorker : ThreadQueueWorker<SiteimproveQueueItem, SiteimproveQueueWorker>
    {
        private readonly ISiteimproveService siteimproveService;


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimproveQueueWorker"/> class.
        /// </summary>
        public SiteimproveQueueWorker()
        {
            this.siteimproveService = Service.Resolve<ISiteimproveService>();
        }


        /// <inheritdoc />
        protected override int DefaultInterval
        {
            get
            {
                return 1000;
            }
        }

        /// <inheritdoc />
        protected override void Finish()
        {
            RunProcess();
        }


        /// <summary>
        /// Enqueues node for recheck.
        /// </summary>
        /// <param name="node">Node to enqueue.</param>
        public static void EnqueueNode(TreeNode node)
        {
            if (node != null && node.HasUrl() && !node.NodeIsSecured)
            {
                Current.Enqueue(new SiteimproveQueueItem() { Node = node }, false);
            }
        }


        /// <summary>
        /// Enqueues nodes for recheck.
        /// </summary>
        /// <param name="nodes">Nodes to enqueue.</param>
        public static void EnqueueNodes(IEnumerable<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                EnqueueNode(node);
            }
        }


        /// <inheritdoc />
        protected override void ProcessItem(SiteimproveQueueItem item)
        {
        }


        /// <inheritdoc />
        protected override int ProcessItems(IEnumerable<SiteimproveQueueItem> items)
        {
            var urls = items.Select(i => DocumentURLProvider.GetAbsoluteUrl(i.Node)).ToList();

            if (urls.Count == 0)
            {
                return 0;
            }

            Task.Run(async () => await siteimproveService.CheckPages(urls));

            return urls.Count;
        }
    }
}
