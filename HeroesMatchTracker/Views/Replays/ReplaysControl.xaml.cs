﻿using HeroesMatchTracker.Core.ViewModels.Replays;
using HeroesMatchTracker.Data;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace HeroesMatchTracker.Views.Replays
{
    /// <summary>
    /// Interaction logic for ReplaysControl.xaml
    /// </summary>
    public partial class ReplaysControl : UserControl
    {
        private IDatabaseService Database;

        public ReplaysControl()
        {
            InitializeComponent();

            Database = ReplaysControlViewModel.GetDatabaseService;

            if (Database.SettingsDb().UserSettings.ReplayAutoStartStartUp)
                ((IInvokeProvider)new ButtonAutomationPeer(StartButton).GetPattern(PatternInterface‌​.Invoke)).Invoke();
        }

        public ReplaysControlViewModel ReplaysControlViewModel
        {
            get { return (ReplaysControlViewModel)DataContext; }
        }
    }
}
