﻿using ERHMS.Desktop.Commands;
using ERHMS.Desktop.Properties;
using ERHMS.Desktop.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ERHMS.Desktop.Views
{
    public partial class MainView : Window
    {
        public new MainViewModel DataContext
        {
            get { return (MainViewModel)base.DataContext; }
            set { base.DataContext = value; }
        }

        public ICommand ExitCommand { get; }

        public MainView()
        {
            InitializeComponent();
            ReadSettings(Settings.Default);
            ExitCommand = new SyncCommand(Exit);
            // TODO: Make sure commands work
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (e.Cancel)
            {
                return;
            }
            WriteSettings(Settings.Default);
            Settings.Default.Save();
        }

        private void ReadSettings(Settings settings)
        {
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            WindowState = settings.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void WriteSettings(Settings settings)
        {
            settings.WindowWidth = RestoreBounds.Width;
            settings.WindowHeight = RestoreBounds.Height;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
        }

        public void Exit()
        {
            Close();
        }
    }
}
