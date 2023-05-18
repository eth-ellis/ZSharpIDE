using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using ZSharpIDE.ViewModels;

namespace ZSharpIDE.Views
{
    public sealed partial class CodeView : Page
    {
        public CodeViewModel ViewModel { get; } = (Application.Current as App).Container.GetService<CodeViewModel>();

        public CodeView()
        {
            this.InitializeComponent();

            this.DataContext = this.ViewModel;
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            var menuFlyout = sender as MenuFlyout;
            
            foreach (var item in menuFlyout.Items)
            {
                item.DataContext = this.ViewModel;
            }
        }

        private void InputBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.ViewModel.SendInputToConsoleCommand.Execute(null);
            }
        }
    }
}
