using Microsoft.UI.Xaml;
using ZSharpIDE.Views;

namespace ZSharpIDE
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Title = "ZSharpIDE";

            this.rootFrame.Navigate(typeof(HomeView));
        }
    }
}
