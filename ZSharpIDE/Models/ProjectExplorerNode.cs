using System.Collections.ObjectModel;

namespace ZSharpIDE.Models
{
    public sealed class ProjectExplorerNode
    {
        public object Content { get; set; }

        public ObservableCollection<ProjectExplorerNode> Children { get; set; } = new ObservableCollection<ProjectExplorerNode>();
    }
}
