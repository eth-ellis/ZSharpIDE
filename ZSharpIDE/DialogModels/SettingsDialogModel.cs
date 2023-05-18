using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ZSharpIDE.Services;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class SettingsDialogModel : ObservableObject
    {
        private readonly SettingsService settingsService = (Application.Current as App).Container.GetService<SettingsService>();

        [ObservableProperty]
        private int selectedSpacesPerTabIndex;

        partial void OnSelectedSpacesPerTabIndexChanging(int value)
        {
            switch (value)
            {
                case 0:
                    this.settingsService.SpacesPerTab = 1;
                    break;
                case 1:
                    this.settingsService.SpacesPerTab = 2;
                    break;
                case 2:
                    this.settingsService.SpacesPerTab = 4;
                    break;
                case 3:
                    this.settingsService.SpacesPerTab = 8;
                    break;
            }
        }

        public SettingsDialogModel()
        {
            switch (this.settingsService.SpacesPerTab)
            {
                case 1:
                    this.SelectedSpacesPerTabIndex = 0;
                    break;
                case 2:
                    this.SelectedSpacesPerTabIndex = 1;
                    break;
                case 4:
                    this.SelectedSpacesPerTabIndex = 2;
                    break;
                case 8:
                    this.SelectedSpacesPerTabIndex = 3;
                    break;
            }
        }
    }
}
