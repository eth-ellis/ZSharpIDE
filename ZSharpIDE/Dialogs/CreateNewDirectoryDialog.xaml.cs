using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class CreateNewDirectoryDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public CreateNewDirectoryDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<CreateNewDirectoryDialogModel>();

        public CreateNewDirectoryDialog(string newDirectoryPath)
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;

            this.DialogModel.NewDirectoryPath = newDirectoryPath;
        }
    }
}
