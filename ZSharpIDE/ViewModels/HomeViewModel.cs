using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ZSharpIDE.Dialogs;
using ZSharpIDE.Extensions;
using ZSharpIDE.Models;
using ZSharpIDE.Services;
using ZSharpIDE.Views;

namespace ZSharpIDE.ViewModels
{
    public sealed partial class HomeViewModel : ObservableObject
    {
        private readonly NavigationService navigationService = (Application.Current as App).Container.GetService<NavigationService>();
        private readonly SettingsService settingsService = (Application.Current as App).Container.GetService<SettingsService>();
        private readonly StateService stateService = (Application.Current as App).Container.GetService<StateService>();
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasRecentProjects))]
        private List<RecentProject> recentProjects = new List<RecentProject>();

        public bool HasRecentProjects
        {
            get => this.RecentProjects.Any();
        }

        public HomeViewModel()
        {
            this.RecentProjects = this.settingsService.RecentProjects;
        }

        [RelayCommand]
        private async Task CreateNewProject()
        {
            await new CreateNewProjectDialog().ShowAsync();
        }

        [RelayCommand]
        private async Task OpenExistingProject()
        {
            var openPicker = new FileOpenPicker();

            var windowHandle = WindowNative.GetWindowHandle(this.appService.MainWindow);

            InitializeWithWindow.Initialize(openPicker, windowHandle);

            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".zsproj");
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                var fileInfo = new FileInfo(file.Path);

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

        [RelayCommand]
        private void OpenRecentProject(RecentProject recentProject)
        {
            var recentProjects = this.settingsService.RecentProjects;
            
            var fileInfo = new FileInfo(recentProject.ProjectPath);

            if (fileInfo.Exists)
            {
                this.stateService.ProjectFile = fileInfo;
                this.stateService.ProjectDirectory = fileInfo.Directory;

                recentProjects.Remove(recentProject);
                recentProjects.Insert(0, recentProject);

                return;
            }

            recentProjects.Remove(recentProject);

            OnPropertyChanged(nameof(RecentProjects));
        }

        [RelayCommand]
        private async Task ConvertExistingCSharpProject()
        {
            var openPicker = new FileOpenPicker();

            var windowHandle = WindowNative.GetWindowHandle(this.appService.MainWindow);

            InitializeWithWindow.Initialize(openPicker, windowHandle);

            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".csproj");
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                var csharpfileInfo = new FileInfo(file.Path);

                var zsharpFileInfo = this.ConvertCSharpProject(csharpfileInfo);

                if (zsharpFileInfo is null)
                {
                    return;
                }

                this.stateService.ProjectFile = zsharpFileInfo;
                this.stateService.ProjectDirectory = zsharpFileInfo.Directory;

                var recentProjects = this.settingsService.RecentProjects;

                if (recentProjects.None(project => Path.GetFileNameWithoutExtension(zsharpFileInfo.Name) == project.ProjectName && zsharpFileInfo.FullName == project.ProjectPath))
                {
                    recentProjects.Insert(0, new RecentProject(Path.GetFileNameWithoutExtension(zsharpFileInfo.Name), zsharpFileInfo.FullName));
                }

                this.settingsService.RecentProjects = recentProjects;

                this.navigationService.Navigate(typeof(CodeView));
            }
        }

        public FileInfo ConvertCSharpProject(FileInfo fileInfo)
        {
            string[] csFiles = Directory.GetFiles(fileInfo.Directory.FullName, "*.cs", SearchOption.AllDirectories);
            string[] csprojFiles = Directory.GetFiles(fileInfo.Directory.FullName, "*.csproj", SearchOption.AllDirectories);

            foreach (var csFile in csFiles)
            {
                var newFilePath = Path.ChangeExtension(csFile, ".zs");
                File.Move(csFile, newFilePath);
            }

            foreach (var csprojFile in csprojFiles)
            {
                var newFilePath = Path.ChangeExtension(csprojFile, ".zsproj");
                File.Move(csprojFile, newFilePath);
            }

            var zsprojFilePath = Directory
                .EnumerateFiles(fileInfo.Directory.FullName)
                .FirstOrDefault(filePath => Path.GetExtension(filePath) == ".zsproj");

            return new FileInfo(zsprojFilePath);
        }
    }
}
