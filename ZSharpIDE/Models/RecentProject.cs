namespace ZSharpIDE.Models
{
    public sealed class RecentProject
    {
        public string ProjectName { get; set; }

        public string ProjectPath { get; set; }

        public RecentProject(string projectName, string projectPath)
        {
            this.ProjectName = projectName;
            this.ProjectPath = projectPath;
        }
    }
}
