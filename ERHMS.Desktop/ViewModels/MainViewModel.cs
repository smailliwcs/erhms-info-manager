﻿using ERHMS.Common;
using ERHMS.Desktop.Commands;
using ERHMS.Desktop.Infrastructure;
using ERHMS.Desktop.Properties;
using ERHMS.Desktop.Services;
using ERHMS.EpiInfo;
using ERHMS.EpiInfo.Projects;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Module = ERHMS.EpiInfo.Module;

namespace ERHMS.Desktop.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public static MainViewModel Current { get; } = new MainViewModel();

        private object content;
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                Log.Default.Debug($"Displaying: {value}");
                SetProperty(ref content, value);
            }
        }

        public Command ExitCommand { get; }
        public Command GoToHomeCommand { get; }
        public Command OpenWorkerProjectCommand { get; }
        public Command OpenIncidentProjectCommand { get; }
        public Command StartEpiInfoCommand { get; }
        public Command StartFileExplorerCommand { get; }

        private MainViewModel()
        {
            ExitCommand = new SimpleSyncCommand(Exit);
            GoToHomeCommand = new SimpleSyncCommand(GoToHome);
            OpenWorkerProjectCommand = new SimpleAsyncCommand(OpenWorkerProject);
            OpenIncidentProjectCommand = new SimpleAsyncCommand(OpenIncidentProject);
            StartEpiInfoCommand = new SimpleSyncCommand(StartEpiInfo);
            StartFileExplorerCommand = new SimpleSyncCommand(StartFileExplorer);
        }

        public event EventHandler ExitRequested;

        private void OnExitRequested()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Exit()
        {
            OnExitRequested();
        }

        private void GoToHome()
        {
            Content = new HomeViewModel();
        }

        private async Task OpenProject(ProjectType projectType, string path)
        {
            // TODO: Error handling
            IProgressService progress = ServiceProvider.GetProgressService("Opening database");
            await progress.RunAsync(() =>
            {
                Content = new ProjectViewModel(ProjectFactory.GetProject(projectType, path));
            });
        }

        private async Task OpenWorkerProject()
        {
            await OpenProject(ProjectType.Worker, Settings.Default.WorkerProjectPath);
        }

        private async Task OpenIncidentProject()
        {
            await OpenProject(ProjectType.Incident, Settings.Default.IncidentProjectPath);
        }

        private void StartEpiInfo()
        {
            Module.Menu.Start();
        }

        private void StartFileExplorer()
        {
            string entryDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Process.Start(entryDirectory);
        }
    }
}
