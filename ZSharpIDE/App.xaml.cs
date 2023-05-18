using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using ZSharpIDE.DialogModels;
using ZSharpIDE.Services;
using ZSharpIDE.ViewModels;

namespace ZSharpIDE
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The Main Window.
        /// </summary>
        public Window MainWindow { get; private set; }

        /// <summary>
        /// The DI Container.
        /// </summary>
        public IServiceProvider Container { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            Current.RequestedTheme = ApplicationTheme.Dark;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.Container = this.RegisterServices();

            this.MainWindow = new MainWindow();
            this.MainWindow.Activate();
        }

        /// <summary>
        /// Initializes the DI container.
        /// </summary>
        /// <returns>An instance implementing IServiceProvider.</returns>
        private IServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            // View Models
            services.AddTransient<HomeViewModel>();
            services.AddTransient<CodeViewModel>();

            // Services
            services.AddSingleton<NavigationService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<StateService>();
            services.AddSingleton<AppService>();

            // Dialog Models
            services.AddTransient<RenameFileDialogModel>();
            services.AddTransient<RenameDirectoryDialogModel>();
            services.AddTransient<CreateNewFileDialogModel>();
            services.AddTransient<CreateNewDirectoryDialogModel>();
            services.AddTransient<CreateNewProjectDialogModel>();
            services.AddTransient<UnsavedContentDialogModel>();

            return services.BuildServiceProvider();
        }
    }
}
