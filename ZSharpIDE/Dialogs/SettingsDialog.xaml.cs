using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public SettingsDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<SettingsDialogModel>();

        public SettingsDialog()
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;
        }
    }
}
