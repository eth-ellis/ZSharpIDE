using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class RenameFileDialogModel : ObservableObject
    {
        [ObservableProperty]
        private FileInfo fileInfo;

        [ObservableProperty]
        private string fileName;

        [RelayCommand]
        private void RenameFile()
        {
            var path = Path.Combine(this.FileInfo.DirectoryName, this.FileName);

            File.Move(this.FileInfo.FullName, path);
        }
    }
}
