using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace ZSharpIDE.Services
{
    public sealed class NavigationService
    {
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public void Navigate(Type pageType)
        {
            this.appService.MainFrame.Navigate(pageType);
        }
    }
}
