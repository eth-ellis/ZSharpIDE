using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class CreateNewProjectDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public CreateNewProjectDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<CreateNewProjectDialogModel>();

        public CreateNewProjectDialog()
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;
        }
    }
}
