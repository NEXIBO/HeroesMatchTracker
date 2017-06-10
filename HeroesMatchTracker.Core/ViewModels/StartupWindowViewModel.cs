﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HeroesMatchTracker.Core.Updater;
using HeroesMatchTracker.Core.ViewServices;
using HeroesMatchTracker.Data;
using Microsoft.Practices.ServiceLocation;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace HeroesMatchTracker.Core.ViewModels
{
    public class StartupWindowViewModel : ViewModelBase
    {
        private Logger StartupLogFile = LogManager.GetLogger(LogFileNames.StartupLogFileName);

        private string _statusLabel;
        private string _detailedStatusLabel;

        private IDatabaseService Database;

        /// <summary>
        /// Constructor
        /// </summary>
        public StartupWindowViewModel(IDatabaseService database)
        {
            Database = database;
        }

        public string AppVersion { get { return AssemblyVersions.HeroesMatchTrackerVersion().ToString(); } }

        public IDatabaseService GetDatabaseService => Database;

        public RelayCommand ExecuteStartupCommand => new RelayCommand(async () => await ExecuteStartup());

        public string StatusLabel
        {
            get => _statusLabel;
            set
            {
                _statusLabel = value;
                RaisePropertyChanged(nameof(StatusLabel));
            }
        }

        public string DetailedStatusLabel
        {
            get => _detailedStatusLabel;
            set
            {
                _detailedStatusLabel = value;
                RaisePropertyChanged(nameof(DetailedStatusLabel));
            }
        }

        public IMainWindowService StartupWindowService => ServiceLocator.Current.GetInstance<IMainWindowService>();

        private async Task ExecuteStartup()
        {
            try
            {
                StatusLabel = "Starting up...";

                await Message("Initializing HeroesMatchTracker.Data");
                var databaseMigrations = Data.Database.Initialize().ExecuteDatabaseMigrations();
                await Message("Performing Database migrations...");
                await databaseMigrations;

#if !DEBUG
                await ApplicationUpdater();
#endif

                await Message("Initializing Heroes Match Tracker");
                StartupWindowService.CreateMainWindow(); // create the main application window
            }
            catch (Exception ex)
            {
                StatusLabel = "An error was encountered. Check the error logs for details";
                StartupLogFile.Log(LogLevel.Error, ex);

                for (int i = 4; i >= 0; i--)
                {
                    DetailedStatusLabel = $"Shutting down in ({i})...";
                    await Task.Delay(1000);
                }

                Application.Current.Shutdown();
            }
        }

        private async Task ApplicationUpdater()
        {
            try
            {
                AutoUpdater autoUpdater = new AutoUpdater(Database);

                await Message("Checking for updates...");

                if (!await autoUpdater.CheckForUpdates())
                {
                    await Message("Already latest version");

                    // make sure we have up to date release notes
                    await Message("Retrieving release notes...");
                    await AutoUpdater.RetrieveReleaseNotes(Database);
                    return;
                }

                if (!Database.SettingsDb().UserSettings.IsAutoUpdates)
                {
                    await Message("Update available, auto-update is disabled");
                    return;
                }

                await Message("Downloading and applying releases...");
                if (!await autoUpdater.ApplyReleases())
                {
                    await Message("Already latest version");
                    return;
                }

                await Message("Retrieving release notes...");
                await AutoUpdater.RetrieveReleaseNotes(Database);

                await Message("Restarting application...");
                await Task.Delay(1000);

                if (Database.SettingsDb().UserSettings.IsWindowsStartup && Database.SettingsDb().UserSettings.IsStartedViaStartup)
                    autoUpdater.RestartApp(arguments: "/noshow /updated");
                else
                    autoUpdater.RestartApp(arguments: "/updated");
            }
            catch (AutoUpdaterException ex)
            {
                await Message("Could not check for updates or apply releases, check logs");
                StartupLogFile.Log(LogLevel.Error, ex);
                await Task.Delay(1000);
            }
        }

        private async Task Message(string message)
        {
            DetailedStatusLabel = message;
            StartupLogFile.Log(LogLevel.Info, message);
            await Task.Delay(1);
        }
    }
}