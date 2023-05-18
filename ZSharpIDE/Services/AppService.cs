using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZSharpIDE.Services
{
    public sealed class AppService
    {
        public App App
        {
            get => Application.Current as App;
        }

        public MainWindow MainWindow
        {
            get => this.App.MainWindow as MainWindow;
        }

        public Frame MainFrame
        {
            get => this.MainWindow.Content as Frame;
        }
    }
}
