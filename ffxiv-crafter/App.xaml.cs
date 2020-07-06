using ffxiv_crafter.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IInitialDataService, InitialDataService>();
            services.AddSingleton<IChildWindowProvider, ChildWindowProvider>();
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<INotificationService, NotificationService>();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var errorText = $"[{DateTimeOffset.Now:O}] ERROR - <{e.Exception.GetType().Name}> {e.Exception.Message}\n{e.Exception.StackTrace}\n";

                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"errors-{DateTimeOffset.Now:yyyy-MM-dd}.log"), errorText);
                MessageBox.Show($"Exception: <{e.Exception.GetType().Name}> {e.Exception.Message}", "Unhandled exception");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Caught exception attempting to log/show unhandled exception: <{ex.GetType().Name}> {ex.Message}");
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();

            mainWindow.Show();
        }
    }
}
