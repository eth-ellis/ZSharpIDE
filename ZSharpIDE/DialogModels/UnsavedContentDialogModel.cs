using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZSharpIDE.Models;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class UnsavedContentDialogModel : ObservableObject
    {
        [ObservableProperty]
        private List<OpenFile> unsavedFiles = new List<OpenFile>();

        [RelayCommand]
        private async Task Save()
        {
            foreach (var file in this.UnsavedFiles)
            {
                await File.WriteAllTextAsync(file.FileInfo.FullName, file.EditContent);
            }
        }
    }
}
