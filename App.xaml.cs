using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ODEliteTracker.Extensions;
using ODEliteTracker.Stores;
using ODEliteTracker.Views;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ODEliteTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static Version AppVersion { get; } = new Version(1, 5, 2);

#if INSTALL || DEBUG
        public readonly static string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ODEliteTracker");
#else
        public readonly static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        private const string database = "ODEliteTracker.db";
        private static readonly string connectionString = $"DataSource={Path.Combine(BaseDirectory, database)};";
        
        private static readonly string CAPIAppName = $"OD Elite Tracker v{AppVersion}";

        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                //Database
                services.AddDatabase(connectionString);
                //Windows
                services.AddWindows();
                //Navigation
                services.AddNavigation();
                //Popout
                services.AddPopOuts();
                //View Models
                services.AddViewModels();
                //Services
                services.AddServices(BaseDirectory);
                //Store
                services.AddStores();
                //http clients
                services.AddHttpClients(CAPIAppName);
            }).Build();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: Path.Combine(BaseDirectory, "Logs", "Error.txt"));
            });

            await _host.StartAsync();

            //Disable shutdown when the dialog closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown; 
            var updateWindow = Services.GetRequiredService<LoaderWindow>();
            if (updateWindow.ShowDialog() is bool v && !v)
            {
                Shutdown(1);
                return;
            }

            var settings = Services.GetRequiredService<SettingsStore>();
            settings.LoadSettings();

            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == 0)
            {
                var settings = Services.GetRequiredService<SettingsStore>();
                settings.SaveSettings();
            }
            await _host.StopAsync();
            _host.Dispose();
        }


        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
            Logger.Fatal(e.Exception.Message);   
            Logger.Fatal(e.Exception.StackTrace);     
        }
    }
}
