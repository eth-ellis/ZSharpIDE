using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class RenameDirectoryDialogModel : ObservableObject
    {
        [ObservableProperty]
        private DirectoryInfo directoryInfo;

        [ObservableProperty]
        private string directoryName;

        [RelayCommand]
        private void RenameDirectory()
        {
            var path = Path.Combine(this.DirectoryInfo.Parent.FullName, this.DirectoryName);

            Directory.Move(this.DirectoryInfo.FullName, path);
        }
    }
}
