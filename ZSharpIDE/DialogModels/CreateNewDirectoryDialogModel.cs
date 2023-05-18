using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class CreateNewDirectoryDialogModel : ObservableObject
    {
        [ObservableProperty]
        private string newDirectoryPath = string.Empty;

        [ObservableProperty]
        private string newDirectoryName = string.Empty;

        [RelayCommand]
        private void CreateNewDirectory()
        {
            var path = Path.Combine(this.NewDirectoryPath, this.NewDirectoryName);

            Directory.CreateDirectory(path);
        }
    }
}
