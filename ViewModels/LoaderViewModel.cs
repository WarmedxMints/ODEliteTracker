using Microsoft.EntityFrameworkCore;
using NLog;
using ODEliteTracker.Database;
using ODMVVM.Helpers.IO;
using ODMVVM.Models;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ODEliteTracker.ViewModels
{
    public sealed class LoaderViewModel : ODObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public LoaderViewModel(ODEliteTrackerDbContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
            _ = Initialise();
        }

        private readonly ODEliteTrackerDbContextFactory contextFactory;
        private string statusText = "Loading";
        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value.ToUpper();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public EventHandler<bool>? InitialiseComplete;

        public async Task Initialise()
        {
            await Task.Delay(1000);
            StatusText = "Checking For App Updates";
            await Task.Delay(1000);

            try
            {
                var updateInfo = await Json.GetJsonFromUrlAndDeserialise<UpdateInfo>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/ODEliteTrackerUpdate.json");

                if (updateInfo.Version > App.AppVersion)
                {
                    var filename = Path.GetFileName(updateInfo.FileUrl);
                    var tempPath = Path.GetTempPath();
                    var filePath = Path.Combine(tempPath, filename);
                    
                    var update = ODDialogService.ShowUpdateBox(null, updateInfo, filePath);

                    if(update == MessageBoxResult.Yes || update == MessageBoxResult.OK)
                    {
                        StatusText = "Closing";
                        await Task.Delay(1000);
                        if (update == MessageBoxResult.Yes) 
                            Process.Start(filePath);

                        Application.Current.Shutdown(1);
                        return;
                    }                    
                }

                if(updateInfo.Version == App.AppVersion)
                {
                    StatusText = "Already on latest version";
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                StatusText = "Error Getting Update";
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                await Task.Delay(1000);
            }
            StatusText = "Migrating Database";
            try
            {
                using var dbContext = contextFactory.CreateDbContext();
                await dbContext.Database.MigrateAsync();
                await Task.Delay(1000);
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                StatusText = "Error Accessing Database\nApplication will now close";
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                await Task.Delay(2000);
                InitialiseComplete?.Invoke(this, false);
                return;
            }
            catch (Exception ex)
            {
                StatusText = "Error Loading\nApplication will now close";
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                await Task.Delay(2000);
                InitialiseComplete?.Invoke(this, false);
                return;
            }

            InitialiseComplete?.Invoke(this, true);
        }
    }
}
