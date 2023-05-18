using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class RenameDirectoryDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public RenameDirectoryDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<RenameDirectoryDialogModel>();

        public RenameDirectoryDialog(DirectoryInfo directoryInfo)
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;

            this.DialogModel.DirectoryInfo = directoryInfo;
            this.DialogModel.DirectoryName = directoryInfo.Name;
        }
    }
}
