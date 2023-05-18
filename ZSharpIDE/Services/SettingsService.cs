using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;
using ZSharpIDE.Models;

namespace ZSharpIDE.Services
{
    public sealed class SettingsService
    {
        private const string RecentProjectsKey = nameof(RecentProjectsKey);
        private const string SpacesPerTabKey = nameof(SpacesPerTabKey);

        /// <summary>
        /// Get or set the recent projects list.
        /// </summary>
        public List<RecentProject> RecentProjects
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                
                if (localSettings.Values.TryGetValue(RecentProjectsKey, out var serialisedSettingValue) &&
                    serialisedSettingValue is string serialisedSetting)
                {
                    return JsonSerializer.Deserialize<List<RecentProject>>(serialisedSetting);
                }

                return new List<RecentProject>();
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                var serialisedSetting = JsonSerializer.Serialize(value);

                localSettings.Values[RecentProjectsKey] = serialisedSetting;
            }
        }

        /// <summary>
        /// Get or set the spaces per tab preference.
        /// </summary>
        public int SpacesPerTab
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                if (localSettings.Values.TryGetValue(SpacesPerTabKey, out var spacesPerTabValue) &&
                    spacesPerTabValue is int spacesPerTab)
                {
                    return spacesPerTab;

                }

                return 4;
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                localSettings.Values[SpacesPerTabKey] = value;
            }
        }
    }
}
