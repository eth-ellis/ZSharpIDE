using System.IO;

namespace ZSharpIDE.Services
{
    public sealed class StateService
    {
        /// <summary>
        /// Get or set the directory containing the project file (.zsproj) and all project resources
        /// </summary>
        public DirectoryInfo ProjectDirectory { get; set; }

        /// <summary>
        /// Get or set the project file (.zsproj)
        /// </summary>
        public FileInfo ProjectFile { get; set; }

        /// <summary>
        /// Get the project name
        /// </summary>
        public string ProjectName
        {
            get => this.ProjectFile?.Name.Replace(".zsproj", "");
        }

        /// <summary>
        /// Get or set the current open file
        /// </summary>
        public FileInfo CurrentOpenFile { get; set; }

        /// <summary>
        /// Get or set the current code font size 
        /// </summary>
        public int CurrentCodeFontSize { get; set; } = 14;
    }
}
