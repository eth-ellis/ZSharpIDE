using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace ZSharpIDE.Models
{
    public sealed partial class OpenFile : ObservableObject
    {
        [ObservableProperty]
        private FileInfo fileInfo;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContentChanged))]
        private string fileContent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContentChanged))]
        private string editContent;

        public bool ContentChanged
        {
            get => this.FileContent != this.EditContent;
        }

        public OpenFile(FileInfo fileInfo, string fileContent)
        {
            this.FileInfo = fileInfo;

            this.FileContent = fileContent;
            this.EditContent = fileContent;
        }
    }
}
