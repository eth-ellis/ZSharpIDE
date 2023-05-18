using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ZSharpIDE.Enums;
using ZSharpIDE.Extensions;
using ZSharpIDE.Models;
using ZSharpIDE.Services;
using ZSharpIDE.Views;

namespace ZSharpIDE.DialogModels
{
    public sealed partial class CreateNewProjectDialogModel : ObservableObject
    {
        private readonly NavigationService navigationService = (Application.Current as App).Container.GetService<NavigationService>();
        private readonly SettingsService settingsService = (Application.Current as App).Container.GetService<SettingsService>();
        private readonly StateService stateService = (Application.Current as App).Container.GetService<StateService>();
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedTemplate))]
        private int selectedTemplateIndex = 0;

        public Template SelectedTemplate
        {
            get
            {
                return (Template)this.SelectedTemplateIndex;
            }
            set
            {
                this.SelectedTemplateIndex = (int)value;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProjectLocation))]
        private string projectName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProjectLocation))]
        private string projectPath = string.Empty;

        [ObservableProperty]
        private bool includeGitIgnore = false;

        [ObservableProperty]
        private bool includeReadme = false;

        [ObservableProperty]
        private bool includeLicence = false;

        public string ProjectLocation
        {
            get => $"Your project will be created in \"{Path.Combine(this.ProjectPath, this.ProjectName)}\"";
        }

        [RelayCommand]
        private async Task OpenFolderPicker()
        {
            FolderPicker openPicker = new FolderPicker();

            var hWnd = WindowNative.GetWindowHandle(this.appService.MainWindow);

            InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                this.ProjectPath = folder.Path;
            }
        }

        [RelayCommand]
        private void CreateNewProject()
        {
            var template = this.SelectedTemplate switch
            {
                Template.Console => "Console",
                Template.WindowsForms => "Windows Forms",
                _ => "Console",
            };

            var projectDirectory = Path.Combine(this.ProjectPath, this.ProjectName);

            Directory.CreateDirectory(projectDirectory);

            var executableFilePath = Assembly.GetEntryAssembly().Location;
            var executableDirectory = Path.GetDirectoryName(executableFilePath);
            var templateDirectory = Path.Combine(executableDirectory, "Resources", "Templates", template);
            var projectFilePath = Path.Combine(projectDirectory, $"{this.ProjectName}.zsproj");
            var codeFilePath = Path.Combine(projectDirectory, $"{this.ProjectName}.zs");

            File.Copy(Path.Combine(templateDirectory, "Template.zsproj"), projectFilePath);
            File.Copy(Path.Combine(templateDirectory, "Template.zs"), codeFilePath);

            if (this.IncludeGitIgnore)
            {
                var gitIgnoreFilePath = Path.Combine(projectDirectory, $".gitignore");

                File.Copy(Path.Combine(templateDirectory, ".gitignore"), gitIgnoreFilePath);
            }

            if (this.IncludeReadme)
            {
                var readmeFilePath = Path.Combine(projectDirectory, $"README.md");

                using (StreamWriter sw = File.CreateText(readmeFilePath))
                {
                    sw.WriteLine($"# {this.ProjectName}");
                }
            }

            if (this.IncludeLicence)
            {
                var licenceFilePath = Path.Combine(projectDirectory, $"LICENSE.txt");

                File.Copy(Path.Combine(templateDirectory, "LICENSE.txt"), licenceFilePath);
            }

            var fileInfo = new FileInfo(projectFilePath);

            this.stateService.ProjectFile = fileInfo;
            this.stateService.ProjectDirectory = fileInfo.Directory;

            var recentProjects = this.settingsService.RecentProjects;

            if (recentProjects.None(project => Path.GetFileNameWithoutExtension(fileInfo.Name) == project.ProjectName && fileInfo.FullName == project.ProjectPath))
            {
                recentProjects.Insert(0, new RecentProject(Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.FullName));
            }

            this.settingsService.RecentProjects = recentProjects;

            this.navigationService.Navigate(typeof(CodeView));
        }
    }
}
