using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;
using ZSharpIDE.Models;

namespace ZSharpIDE.Services
{
    public sealed class SettingsService
    {
        private const string RecentProjectsKey = nameof(RecentProjectsKey);

        /// <summary>
        /// Get or set the recent projects list.
        /// </summary>
        /// <remarks>
        /// Value 1 is the project name. Value 2 is the project file path.
        /// </remarks>
        public List<RecentProject> RecentProjects
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                
                if (localSettings.Values.TryGetValue(RecentProjectsKey, out var serialisedSetting))
                {
                    if (serialisedSetting is string)
                    {
                        return JsonSerializer.Deserialize<List<RecentProject>>(serialisedSetting as string);
                    }
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
    }
}
