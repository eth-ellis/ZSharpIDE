using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZSharpIDE.Dialogs;
using ZSharpIDE.Enums;
using ZSharpIDE.Extensions;
using ZSharpIDE.Models;
using ZSharpIDE.Services;
using ZSharpIDE.Views;

namespace ZSharpIDE.ViewModels
{
    public class ProjectExplorerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ProjectTemplate { get; set; }
        public DataTemplate DirectoryTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var node = item as ProjectExplorerNode;

            if (node is null)
            {
                return null;
            }

            if (node.Content is DirectoryInfo)
            {
                return this.DirectoryTemplate;
            }

            if (node.Content is FileInfo fileInfo)
            {
                if (fileInfo.Extension == ".zsproj")
                {
                    return this.ProjectTemplate;
                }
                else
                {
                    return this.FileTemplate;
                };
            }

            return null;
        }
    }

    public sealed partial class CodeViewModel : ObservableObject
    {
        private readonly NavigationService navigationService = (Application.Current as App).Container.GetService<NavigationService>();
        private readonly StateService stateService = (Application.Current as App).Container.GetService<StateService>();
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        private Process buildProcess;
        private Process executeProcess;

        private CancellationTokenSource cancellationTokenSource;

        private FileSystemWatcher watcher;

        private bool canClose = false;

        [ObservableProperty]
        private bool programRunning = false;

        [ObservableProperty]
        private string inputText = "";

        [ObservableProperty]
        private bool showHiddenFiles = false;

        [ObservableProperty]
        private OpenFile selectedFileTab;

        partial void OnSelectedFileTabChanging(OpenFile value)
        {
            if (value is not null)
            {
                this.stateService.CurrentOpenFile = value.FileInfo;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedConfiguration))]
        private int selectedConfigurationIndex = 0;

        public Configuration SelectedConfiguration
        {
            get
            {
                return (Configuration)this.SelectedConfigurationIndex;
            }
            set
            {
                this.SelectedConfigurationIndex = (int)value;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedBottomTab))]
        private int selectedBottomTabIndex = 0;

        public BottomTab SelectedBottomTab
        {
            get
            {
                return (BottomTab)this.SelectedBottomTabIndex;
            }
            set
            {
                this.SelectedBottomTabIndex = (int)value;
            }
        }

        public ObservableCollection<ProjectExplorerNode> ProjectExplorerNodes { get; set; } = new ObservableCollection<ProjectExplorerNode>();

        public ObservableCollection<OpenFile> OpenFiles { get; set; } = new ObservableCollection<OpenFile>();

        public ObservableCollection<string> BuildLines { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> OutputLines { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Error> ErrorLines { get; set; } = new ObservableCollection<Error>();

        public int ErrorCount
        {
            get => this.ErrorLines.Where(error => error.Severity == Severity.Error).Count();
        }

        public int WarningCount
        {
            get => this.ErrorLines.Where(error => error.Severity == Severity.Warning).Count();
        }

        public CodeViewModel()
        {
            this.ProjectExplorerNodes.Add(this.GetProjectExplorerRootNode());

            this.appService.MainWindow.Closed += MainWindow_Closed;

            this.watcher = new FileSystemWatcher(this.stateService.ProjectDirectory.FullName);

            this.watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

            this.watcher.Changed += (sender, e) => this.UpdateProjectExplorerAndFileTabs();
            this.watcher.Created += (sender, e) => this.UpdateProjectExplorerAndFileTabs();
            this.watcher.Deleted += (sender, e) => this.UpdateProjectExplorerAndFileTabs();
            this.watcher.Renamed += (sender, e) => this.UpdateProjectExplorerAndFileTabs();

            this.watcher.IncludeSubdirectories = true;
            this.watcher.EnableRaisingEvents = true;
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (this.canClose)
            {
                return;
            }

            args.Handled = true;

            var unsavedFiles = this.OpenFiles.Where(file => file.ContentChanged).ToList();

            if (unsavedFiles.Any())
            {
                var unsavedContentDialog = await new UnsavedContentDialog(unsavedFiles).ShowAsync();

                if (unsavedContentDialog == ContentDialogResult.Primary ||
                    unsavedContentDialog == ContentDialogResult.Secondary)
                {
                    this.canClose = true;
                    this.appService.MainWindow.Close();
                }
            }
            else
            {
                this.canClose = true;
                this.appService.MainWindow.Close();
            }
        }

        [RelayCommand]
        private async Task ReturnHome()
        {
            var unsavedFiles = this.OpenFiles.Where(file => file.ContentChanged).ToList();

            if (unsavedFiles.Any())
            {
                var unsavedContentDialog = await new UnsavedContentDialog(unsavedFiles).ShowAsync();

                if (unsavedContentDialog == ContentDialogResult.Primary ||
                    unsavedContentDialog == ContentDialogResult.Secondary)
                {
                    this.appService.MainWindow.Closed -= MainWindow_Closed;
                    this.navigationService.Navigate(typeof(HomeView));
                }
            }
            else
            {
                this.appService.MainWindow.Closed -= MainWindow_Closed;
                this.navigationService.Navigate(typeof(HomeView));
            }
        }

        [RelayCommand]
        private async Task OpenFile(FileInfo fileInfo)
        {
            this.stateService.CurrentOpenFile = fileInfo;

            var alreadyOpenFile = this.OpenFiles.FirstOrDefault(fileTab => fileTab.FileInfo.FullName == fileInfo.FullName);
            
            if (alreadyOpenFile is not null)
            {
                this.SelectedFileTab = alreadyOpenFile;

                return;
            }

            var fileContent = await File.ReadAllTextAsync(fileInfo.FullName);

            var openFile = new OpenFile(fileInfo, fileContent);

            this.OpenFiles.Insert(0, openFile);

            // HACK - Fixes issue where file content is not displayed until
            // the tab is switched to another tab and then back.
            await Task.Delay(10);

            this.SelectedFileTab = openFile;
        }

        [RelayCommand]
        private async Task SaveCurrentFile()
        {
            var currentOpenFile = this.OpenFiles.FirstOrDefault(file => file.FileInfo.FullName == this.stateService.CurrentOpenFile.FullName);

            await File.WriteAllTextAsync(currentOpenFile.FileInfo.FullName, currentOpenFile.EditContent);

            currentOpenFile.FileContent = currentOpenFile.EditContent;
        }

        [RelayCommand]
        private async Task CloseTab(TabViewTabCloseRequestedEventArgs eventArgs)
        {
            if (eventArgs.Item is OpenFile openFile)
            {
                if (openFile.ContentChanged)
                {
                    var unsavedContentDialog = await new UnsavedContentDialog(new List<OpenFile>() { openFile }).ShowAsync();

                    if (unsavedContentDialog == ContentDialogResult.Primary ||
                        unsavedContentDialog == ContentDialogResult.Secondary)
                    {
                        this.OpenFiles.Remove(openFile);
                    }
                }
                else
                {
                    this.OpenFiles.Remove(openFile);
                }
            }
        }

        [RelayCommand]
        private async Task OpenGitConsole()
        {
            var gitConsoleProcess = new Process();

            gitConsoleProcess.StartInfo.FileName = "cmd.exe";
            gitConsoleProcess.StartInfo.WorkingDirectory = this.stateService.ProjectDirectory.FullName;

            gitConsoleProcess.Start();

            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await gitConsoleProcess.WaitForExitAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                gitConsoleProcess.Kill();
                return;
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        #region Program Operation

        [RelayCommand]
        private async Task RunProgram()
        {
            this.ProgramRunning = true;

            this.BuildLines.Clear();
            this.OutputLines.Clear();

            this.ErrorLines.Clear();
            OnPropertyChanged(nameof(ErrorCount));
            OnPropertyChanged(nameof(WarningCount));

            await this.RunBuildProcess();

            if (this.buildProcess.ExitCode == 0)
            {
                await this.RunExecuteProcess();
            }

            this.ProgramRunning = false;
        }

        private async Task RunBuildProcess()
        {
            this.SelectedBottomTab = BottomTab.Build;

            var tempDirectory = Path.Combine(this.stateService.ProjectDirectory.FullName, "tmp");

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            Directory.CreateDirectory(tempDirectory);

            var copyDirectories = this.stateService.ProjectDirectory
                .GetDirectories()
                .Where(directory => directory.Name != "tmp"
                                 && directory.Name != "bin");

            foreach (var directory in copyDirectories)
            {
                FileSystem.CopyDirectory(directory.FullName, Path.Combine(tempDirectory, directory.Name));
            }

            var copyFiles = this.stateService.ProjectDirectory
                .GetFiles()
                .Where(file => file.Name != ".gitignore"
                            && file.Name != "README.md"
                            && file.Name != "LICENSE.txt");

            foreach (var file in copyFiles)
            {
                File.Copy(file.FullName, Path.Combine(tempDirectory, file.Name));
            }

            string[] zsFiles = Directory.GetFiles(tempDirectory, "*.zs", System.IO.SearchOption.AllDirectories);
            string[] zsprojFiles = Directory.GetFiles(tempDirectory, "*.zsproj", System.IO.SearchOption.AllDirectories);

            foreach (var zsFile in zsFiles)
            {
                string newFilePath = Path.ChangeExtension(zsFile, ".cs");
                File.Move(zsFile, newFilePath);
            }

            foreach (var zsprojFile in zsprojFiles)
            {
                string newFilePath = Path.ChangeExtension(zsprojFile, ".csproj");
                File.Move(zsprojFile, newFilePath);
            }

            var csprojFilePath = Path.Combine(tempDirectory, $"{this.stateService.ProjectName}.csproj");
            var outputDirectoryPath = Path.Combine(this.stateService.ProjectDirectory.FullName, "bin", this.SelectedConfiguration.ToString());

            if (Directory.Exists(outputDirectoryPath))
            {
                Directory.Delete(outputDirectoryPath, true);
            }

            Directory.CreateDirectory(outputDirectoryPath);

            this.buildProcess = new Process();
            this.buildProcess.StartInfo.FileName = "cmd.exe";
            this.buildProcess.StartInfo.Arguments = $"/c dotnet build \"{csprojFilePath}\" --configuration {this.SelectedConfiguration} --property:OutputPath=\"{outputDirectoryPath}\"";
            this.buildProcess.StartInfo.CreateNoWindow = true;
            this.buildProcess.StartInfo.RedirectStandardError = true;
            this.buildProcess.StartInfo.RedirectStandardOutput = true;
            this.buildProcess.StartInfo.RedirectStandardInput = true;

            this.buildProcess.OutputDataReceived += HandleOutputDataReceivedFromBuildProcess;
            this.buildProcess.ErrorDataReceived += HandleErrorDataReceivedFromBuildProcess;

            this.buildProcess.Start();
            this.buildProcess.BeginErrorReadLine();
            this.buildProcess.BeginOutputReadLine();

            this.cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.buildProcess.WaitForExitAsync(this.cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                this.buildProcess.Kill();
                this.ProgramRunning = false;
                return;
            }
            finally
            {
                this.cancellationTokenSource.Dispose();
            }
        }

        private async Task RunExecuteProcess()
        {
            this.SelectedBottomTab = BottomTab.Output;

            var outputDirectoryPath = Path.Combine(this.stateService.ProjectDirectory.FullName, "bin", this.SelectedConfiguration.ToString());
            var executablePath = Path.Combine(outputDirectoryPath, $"{this.stateService.ProjectName}.exe");

            this.executeProcess = new Process();
            this.executeProcess.StartInfo.FileName = executablePath;
            this.executeProcess.StartInfo.CreateNoWindow = true;
            this.executeProcess.StartInfo.RedirectStandardError = true;
            this.executeProcess.StartInfo.RedirectStandardOutput = true;
            this.executeProcess.StartInfo.RedirectStandardInput = true;

            this.executeProcess.OutputDataReceived += HandleOutputDataReceivedFromExecuteProcess;
            this.executeProcess.ErrorDataReceived += HandleErrorDataReceivedFromExecuteProcess;

            this.executeProcess.Start();
            this.executeProcess.BeginErrorReadLine();
            this.executeProcess.BeginOutputReadLine();

            this.cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.executeProcess.WaitForExitAsync(this.cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                this.executeProcess.Kill();
                this.ProgramRunning = false;
                return;
            }
            finally
            {
                this.cancellationTokenSource.Dispose();
            }
        }

        private void HandleOutputDataReceivedFromBuildProcess(object sender, DataReceivedEventArgs e)
        {
            this.appService.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                var errorMatch = Regex.Match(e.Data, @"^(.+)\((\d+),(\d+)\): error (\w+): ([^[]+)");

                if (errorMatch.Success)
                {
                    string code = errorMatch.Groups[4].Value;
                    string description = errorMatch.Groups[5].Value.Trim();
                    string filePath = errorMatch.Groups[1].Value;
                    int line = int.Parse(errorMatch.Groups[2].Value);

                    var error = new Error(Severity.Error, code, description, filePath, line);

                    if (this.ErrorLines.None(line =>
                        line.Severity == error.Severity &&
                        line.Code == error.Code &&
                        line.Description == error.Description &&
                        line.FilePath == error.FilePath &&
                        line.Line == error.Line))
                    {
                        this.ErrorLines.Add(error);
                    }

                    OnPropertyChanged(nameof(ErrorCount));
                }

                var warningMatch = Regex.Match(e.Data, @"^(.+)\((\d+),(\d+)\): warning (\w+): ([^[]+)");

                if (warningMatch.Success)
                {
                    string code = warningMatch.Groups[4].Value;
                    string description = warningMatch.Groups[5].Value.Trim();
                    string filePath = warningMatch.Groups[1].Value;
                    int line = int.Parse(warningMatch.Groups[2].Value);

                    var error = new Error(Severity.Warning, code, description, filePath, line);
                    
                    if (this.ErrorLines.None(line =>
                        line.Severity == error.Severity &&
                        line.Code == error.Code &&
                        line.Description == error.Description &&
                        line.FilePath == error.FilePath &&
                        line.Line == error.Line))
                    {
                        this.ErrorLines.Add(error);
                    }

                    OnPropertyChanged(nameof(WarningCount));
                }

                if (errorMatch.Success || warningMatch.Success)
                {
                    return;
                }

                this.BuildLines.Add(e.Data);
            });
        }

        private void HandleErrorDataReceivedFromBuildProcess(object sender, DataReceivedEventArgs e)
        {
            this.appService.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                this.BuildLines.Add(e.Data);
            });
        }

        private void HandleOutputDataReceivedFromExecuteProcess(object sender, DataReceivedEventArgs e)
        {
            this.appService.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                this.OutputLines.Add(e.Data);
            });
        }

        private void HandleErrorDataReceivedFromExecuteProcess(object sender, DataReceivedEventArgs e)
        {
            this.appService.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                this.OutputLines.Add(e.Data);
            });
        }

        [RelayCommand]
        private void StopProgram()
        {
            this.cancellationTokenSource.Cancel();
        }

        [RelayCommand]
        private async Task SendInputToConsole()
        {
            await this.executeProcess.StandardInput.WriteLineAsync(this.InputText);

            this.InputText = "";
        }

        #endregion

        #region Project Explorer

        [RelayCommand]
        private void RevealInFileExplorer(object info)
        {
            if (info is FileInfo fileInfo)
            {
                Process.Start("explorer.exe", $"/select, \"{fileInfo.FullName}\"");
            }

            if (info is DirectoryInfo directoryInfo)
            {
                Process.Start("explorer.exe", $"/select, \"{directoryInfo.FullName}\"");
            }
        }

        [RelayCommand]
        private async Task NewFile(object info)
        {
            string newFilePath;

            if (info is FileInfo fileInfo)
            {
                newFilePath = fileInfo.DirectoryName;
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                newFilePath = directoryInfo.FullName;
            }
            else
            {
                return;
            }

            await new CreateNewFileDialog(newFilePath).ShowAsync();
        }

        [RelayCommand]
        private async Task NewDirectory(object info)
        {
            string newDirectoryPath;

            if (info is FileInfo fileInfo)
            {
                newDirectoryPath = fileInfo.DirectoryName;
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                newDirectoryPath = directoryInfo.FullName;
            }
            else
            {
                return;
            }

            await new CreateNewDirectoryDialog(newDirectoryPath).ShowAsync();
        }

        [RelayCommand]
        private async Task RenameFile(FileInfo fileInfo)
        {
            await new RenameFileDialog(fileInfo).ShowAsync();
        }

        [RelayCommand]
        private async Task RenameDirectory(DirectoryInfo directoryInfo)
        {
            await new RenameDirectoryDialog(directoryInfo).ShowAsync();
        }

        [RelayCommand]
        private async Task DeleteFile(FileInfo fileInfo)
        {
            var dialog = new ContentDialog();

            dialog.Title = "Are you sure you want to delete this file?";
            dialog.CloseButtonText = "Cancel";
            dialog.PrimaryButtonText = "Yes, I'm sure";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.XamlRoot = this.appService.MainFrame.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                File.Delete(fileInfo.FullName);
            }
        }

        [RelayCommand]
        private async Task DeleteDirectory(DirectoryInfo directoryInfo)
        {
            var dialog = new ContentDialog();

            dialog.Title = "Are you sure you want to delete this folder and all it's contents?";
            dialog.CloseButtonText = "Cancel";
            dialog.PrimaryButtonText = "Yes, I'm sure";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.XamlRoot = this.appService.MainFrame.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Directory.Delete(directoryInfo.FullName, true);
            }
        }

        [RelayCommand]
        private void RefreshProjectExplorer()
        {
            this.ProjectExplorerNodes.Clear();
            this.ProjectExplorerNodes.Add(this.GetProjectExplorerRootNode());
        }

        private ProjectExplorerNode GetProjectExplorerRootNode()
        {
            var rootNode = new ProjectExplorerNode()
            {
                Content = this.stateService.ProjectFile,
            };

            this.AddNodesFromDirectoryToParent(new DirectoryInfo(this.stateService.ProjectDirectory.FullName), rootNode);

            return rootNode;
        }

        private void AddNodesFromDirectoryToParent(DirectoryInfo directoryInfo, ProjectExplorerNode parent)
        {
            var displaySubdirectories = directoryInfo.GetDirectories().Where(directory => directory.Name != "tmp" && directory.Name != "bin");

            foreach (var subdirectoryInfo in displaySubdirectories)
            {
                var directoryNode = new ProjectExplorerNode()
                {
                    Content = subdirectoryInfo
                };

                parent.Children.Add(directoryNode);

                this.AddNodesFromDirectoryToParent(subdirectoryInfo, directoryNode);
            }

            var displayFiles = this.ShowHiddenFiles
                ? directoryInfo.GetFiles().Where(file => file.Extension != ".zsproj")
                : directoryInfo.GetFiles().Where(file => file.Extension != ".zsproj" && file.Name != ".gitignore" && file.Name != "README.md" && file.Name != "LICENSE.txt");

            foreach (var fileInfo in displayFiles)
            {
                var fileNode = new ProjectExplorerNode()
                {
                    Content = fileInfo
                };

                parent.Children.Add(fileNode);
            }
        }

        private void UpdateProjectExplorerAndFileTabs()
        {
            this.appService.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                this.ProjectExplorerNodes.Clear();
                this.ProjectExplorerNodes.Add(this.GetProjectExplorerRootNode());

                foreach (var file in this.OpenFiles.Where(file => !File.Exists(file.FileInfo.FullName)))
                {
                    this.OpenFiles.Remove(file);
                }
            });
        }

        #endregion
    }
}