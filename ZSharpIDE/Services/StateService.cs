using System.IO;

namespace ZSharpIDE.Services
{
    public sealed class StateService
    {
        /// <summary>
        /// The directory containing the project file (.zsproj) and all project resources
        /// </summary>
        public DirectoryInfo ProjectDirectory { get; set; }

        /// <summary>
        /// The project file (.zsproj)
        /// </summary>
        public FileInfo ProjectFile { get; set; }

        /// <summary>
        /// The project name
        /// </summary>
        public string ProjectName
        {
            get => this.ProjectFile?.Name.Replace(".zsproj", "");
        }

        /// <summary>
        /// The current open file
        /// </summary>
        public FileInfo CurrentOpenFile { get; set; }
    }
}
