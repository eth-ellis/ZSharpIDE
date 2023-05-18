using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class RenameFileDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public RenameFileDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<RenameFileDialogModel>();

        public RenameFileDialog(FileInfo fileInfo)
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;

            this.DialogModel.FileInfo = fileInfo;
            this.DialogModel.FileName = fileInfo.Name;
        }
    }
}
