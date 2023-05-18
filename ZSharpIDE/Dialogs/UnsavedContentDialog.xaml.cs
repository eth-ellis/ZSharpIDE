using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Models;
using ZSharpIDE.Services;

namespace ZSharpIDE.Dialogs
{
    public sealed partial class UnsavedContentDialog : ContentDialog
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public UnsavedContentDialogModel DialogModel { get; } = (Application.Current as App).Container.GetService<UnsavedContentDialogModel>();

        public UnsavedContentDialog(List<OpenFile> unsavedFiles)
        {
            this.InitializeComponent();

            this.XamlRoot = this.appService.MainFrame.XamlRoot;

            this.DialogModel.UnsavedFiles = unsavedFiles;
        }
    }
}
