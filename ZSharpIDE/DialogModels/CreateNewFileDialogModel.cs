using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class CreateNewFileDialogModel : ObservableObject
    {
        [ObservableProperty]
        private string newFilePath = string.Empty;

        [ObservableProperty]
        private string newFileName = string.Empty;

        [RelayCommand]
        private void CreateNewFile()
        {
            var path = Path.Combine(this.NewFilePath, this.NewFileName);

            File.Create(path)
                .Close();
        }
    }
}
