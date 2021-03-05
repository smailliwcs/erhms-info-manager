﻿using ERHMS.Common;
using ERHMS.Desktop.Commands;
using ERHMS.Desktop.Infrastructure;
using ERHMS.Desktop.Services;
using ERHMS.EpiInfo;
using log4net.Appender;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Module = ERHMS.EpiInfo.Module;

namespace ERHMS.Desktop.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public static MainViewModel Instance { get; } = new MainViewModel();

        private static string GetLogFilePath()
        {
            return Log.Default.Logger.Repository.GetAppenders()
                .OfType<FileAppender>()
                .Single()
                .File;
        }

        private static string GetLogDirectoryPath()
        {
            return Path.GetDirectoryName(GetLogFilePath());
        }

        private object content;
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                Log.Default.Debug($"Displaying content: {value}");
                SetProperty(ref content, value);
            }
        }

        public ICommand ExitCommand { get; }
        public ICommand ViewHomeCommand { get; }
        public ICommand ViewLogCommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand StartEpiInfoCommand { get; }
        public ICommand StartFileExplorerCommand { get; }
        public ICommand StartCommandPromptCommand { get; }

        private MainViewModel()
        {
            ExitCommand = new SyncCommand(Exit);
            ViewHomeCommand = new SyncCommand(ViewHome);
            ViewLogCommand = new SyncCommand(ViewLog);
            ViewLogsCommand = new SyncCommand(ViewLogs);
            ExportLogsCommand = new SyncCommand(ExportLogs);
            StartEpiInfoCommand = new SyncCommand(StartEpiInfo);
            StartFileExplorerCommand = new SyncCommand(StartFileExplorer);
            StartCommandPromptCommand = new SyncCommand(StartCommandPrompt);
        }

        public event EventHandler ExitRequested;
        private void OnExitRequested(EventArgs e) => ExitRequested?.Invoke(this, e);
        private void OnExitRequested() => OnExitRequested(EventArgs.Empty);

        public void Exit()
        {
            OnExitRequested();
        }

        public void ViewHome()
        {
            Content = new HomeViewModel();
        }

        public void ViewLog()
        {
            Process.Start(GetLogFilePath());
        }

        public void ViewLogs()
        {
            Process.Start(GetLogDirectoryPath());
        }

        public void ExportLogs()
        {
            bool? result = ServiceProvider.Resolve<IFileDialogService>().Save(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Logs-{DateTime.Now:yyyyMMdd-HHmmss}.zip",
                "ZIP Files (*.zip)|*.zip",
                out string path);
            if (result == true)
            {
                ZipExtensions.CreateFromDirectory(GetLogDirectoryPath(), path);
            }
        }

        public void StartEpiInfo()
        {
            Module.Menu.Start();
        }

        public void StartFileExplorer()
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        public void StartCommandPrompt()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Environment.GetEnvironmentVariable("ComSpec")
            });
        }
    }
}
