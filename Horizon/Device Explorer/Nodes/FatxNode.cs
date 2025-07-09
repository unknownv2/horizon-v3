using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDev.XContent;
using DevComponents.AdvTree;

namespace NoDev.Horizon.DeviceExplorer
{
    abstract class FatxNode : Node
    {
        internal readonly FatxDevice Device;

        internal const string LineBreak = "<br></br>";

        internal static string CreateGrayText(string text)
        {
            return "<font color=\"#7D7974\">" + text + "</font>";
        }

        protected FatxNode(FatxDevice device, bool expandable)
        {
            this.Device = device;
            this.Cells.Add(new Cell());

            this.ExpandVisibility = expandable ? eNodeExpandVisibility.Visible : eNodeExpandVisibility.Hidden;
        }

        internal void Invoke(Action function)
        {
            this.TreeControl.Invoke(function);
        }

        internal static void Event_BeforeCellEdit(object sender, CellEditEventArgs e)
        {
            if (e.Cell.Parent is FatxNode)
                ((FatxNode)e.Cell.Parent).BeforeCellEdit(e);
        }

        internal static void Event_AfterCellEdit(object sender, CellEditEventArgs e)
        {
            if (e.Cell.Parent is FatxNode)
                ((FatxNode)e.Cell.Parent).AfterCellEdit(e);
        }

        internal static void Event_BeforeExpand(object sender, AdvTreeNodeCancelEventArgs e)
        {
            if (e.Node is FatxNode && e.Node.Nodes.Count == 0)
                ((FatxNode)e.Node).BeforeExpand();
        }

        internal static void Event_BeforeNodeInsert(object sender, TreeNodeCollectionEventArgs e)
        {
            var node = e.Node as FatxNode;
            if (node != null)
            {
                node.UpdateCells();
                node.UpdateImage();
            }
        }

        internal virtual Task GetPackages(List<XContentPackage> packages)
        {
            throw new NotImplementedException();
        }

        protected virtual void BeforeExpand()
        {
            
        }

        protected virtual void BeforeCellEdit(CellEditEventArgs e)
        {

        }

        protected virtual void AfterCellEdit(CellEditEventArgs e)
        {

        }

        internal abstract string ExtractText { get; }

        protected bool RemovedDisabledNodes;
        protected void OnAddingFinishedDefault()
        {
            this.RemoveDisabledNodes();

            if (this.Nodes.Count == 0)
                this.Nodes.Add(NoContentNode);
        }

        protected void RemoveDisabledNodes()
        {
            if (this.RemovedDisabledNodes)
                return;

            for (int x = 0; x < this.Nodes.Count; x++)
            {
                if (this.Nodes[x].Selectable)
                    continue;
                
                this.Nodes.RemoveAt(x);
                x--;
            }

            this.RemovedDisabledNodes = true;
        }

        private static Node CreateDisabledNode(string text)
        {
            return new Node(text)
            {
                Selectable = false,
                ExpandVisibility = eNodeExpandVisibility.Hidden
            };
        }

        protected static Node LoadingNode
        {
            get
            {
                return CreateDisabledNode("Loading...");
            }
        }

        internal static Node NoContentNode
        {
            get
            {
                return CreateDisabledNode("No Content");
            }
        }

        internal abstract void UpdateImage();
        internal abstract void UpdateCells();
    }
}
