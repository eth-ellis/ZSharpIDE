using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class CreateNewFileDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public CreateNewFileDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<CreateNewFileDialogModel>();

        public CreateNewFileDialog(string newFilePath)
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;

            this.DialogModel.NewFilePath = newFilePath;
        }
    }
}
