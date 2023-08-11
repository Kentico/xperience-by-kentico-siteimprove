using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using CMS.Helpers;

using Kentico.Xperience.Siteimprove;

[assembly: RegisterModule(typeof(SiteimproveModule))]

namespace Kentico.Xperience.Siteimprove
{
    /// <summary>
    /// Represents the SiteimproveModule. 
    /// Responsible for handling page publish and page URL change events.
    /// </summary>
    internal class SiteimproveModule : Module
    {
        private readonly string urlPropertiesQueryName = $"{PageUrlPathInfo.TYPEINFO.ObjectClassName}.UpdateUrlPaths";


        /// <summary>
        /// Initializes an instance of the <see cref="SiteimproveModule"/> class.
        /// </summary>
        public SiteimproveModule() : base(nameof(SiteimproveModule))
        {
        }


        /// <inheritdoc/>
        protected override void OnInit()
        {
            base.OnInit();

            WorkflowEvents.Publish.After += Workflow_PublishAfter;
            DocumentEvents.Move.After += Document_MoveAfter;
            SqlEvents.ExecuteQuery.After += Sql_ExecuteQueryAfter;

            RequestEvents.RunEndRequestTasks.Execute += Request_RunEndRequestTasksExecute;

            ApplicationEvents.PostStart.Execute += Application_PostStartExecute;
        }


        private void Workflow_PublishAfter(object sender, WorkflowEventArgs e)
        {
            var node = e.PublishedDocument;

            SiteimproveQueueWorker.EnqueueNode(node);
        }


        private void Document_MoveAfter(object sender, DocumentEventArgs e)
        {
            var movedNode = e.Node;

            var nodes = GetMovedNodes(movedNode.NodeAliasPath, movedNode.NodeSiteID);

            SiteimproveQueueWorker.EnqueueNodes(nodes);
        }


        private void Sql_ExecuteQueryAfter(object sender, ExecuteQueryEventArgs<DataSet> e)
        {
            var query = e.Query;

            if (!string.Equals(query.Name, urlPropertiesQueryName, StringComparison.Ordinal))
            {
                return;
            }

            // Update not successful, do not order recheck
            if (!DataHelper.DataSourceIsEmpty(e.Result))
            {
                return;
            }

            var newPathParam = query.Params["NewUrlPath"];

            var newSiteIDParam = query.Params["NewSiteID"];

            var nodes = GetUpdatedNodes($"{newPathParam.Value}", (int)newSiteIDParam.Value);

            SiteimproveQueueWorker.EnqueueNodes(nodes);
        }


        private void Request_RunEndRequestTasksExecute(object sender, EventArgs e)
        {
            SiteimproveQueueWorker.Current.EnsureRunningThread();
        }


        private async void Application_PostStartExecute(object sender, EventArgs args)
        {
            var siteimproveService = Service.Resolve<ISiteimproveService>();

            await siteimproveService.Initialize();
        }


        private IEnumerable<TreeNode> GetMovedNodes(string path, int siteID)
        {
            return new MultiDocumentQuery()
                .OnSite(siteID)
                .Published()
                .WhereEquals("NodeAliasPath", path)
                .Or()
                .WhereStartsWith("NodeAliasPath", path.TrimEnd('/') + '/')
                .WithPageUrlPaths()
                .GetEnumerableTypedResult();
        }


        private IEnumerable<TreeNode> GetUpdatedNodes(string path, int siteID)
        {
            return new MultiDocumentQuery()
                .OnSite(siteID)
                .Published()
                .WithPageUrlPaths()
                .WhereEquals("PageUrlPathUrlPath", path)
                .Or()
                .WhereStartsWith("PageUrlPathUrlPath", path + "/")
                .GetEnumerableTypedResult();
        }
    }
}
