using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZSharpIDE.ViewModels;

namespace ZSharpIDE.Views
{
    public sealed partial class HomeView : Page
    {
        public HomeViewModel ViewModel { get; } = (Application.Current as App).Container.GetService<HomeViewModel>();

        public HomeView()
        {
            this.InitializeComponent();

            this.DataContext = this.ViewModel;
        }
    }
}
