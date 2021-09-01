﻿using Epi;
using ERHMS.Common.ComponentModel;
using ERHMS.Common.Compression;
using ERHMS.Common.Logging;
using ERHMS.Desktop.Commands;
using ERHMS.Desktop.Dialogs;
using ERHMS.Desktop.Properties;
using ERHMS.Desktop.Services;
using ERHMS.Desktop.ViewModels.Wizards;
using ERHMS.Domain;
using ERHMS.EpiInfo;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Module = ERHMS.EpiInfo.Module;
using Settings = ERHMS.Desktop.Properties.Settings;

namespace ERHMS.Desktop.ViewModels
{
    public class MainViewModel : ObservableObject, IAppCommands
    {
        private object content;
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                Log.Instance.Debug($"Navigating: {value}");
                SetProperty(ref content, value);
            }
        }

        public HomeViewModel Home { get; } = new HomeViewModel();
        public HelpViewModel Help { get; } = new HelpViewModel();
        public StartViewModel Start { get; } = new StartViewModel();
        public AboutViewModel About { get; } = new AboutViewModel();

        public ICommand GoToHomeCommand { get; }
        public ICommand GoToHelpCommand { get; }
        public ICommand GoToStartCommand { get; }
        public ICommand GoToAboutCommand { get; }
        public ICommand GoToProjectCommand { get; }
        public ICommand GoToCoreProjectCommand { get; }
        public ICommand GoToViewCommand { get; }
        public ICommand GoToCoreViewCommand { get; }
        public ICommand CreateCoreProjectCommand { get; }
        public ICommand OpenCoreProjectCommand { get; }
        public ICommand SetUpCoreProjectCommand { get; }
        public ICommand OpenUriCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand StartEpiInfoCommand { get; }
        public ICommand StartCommandPromptCommand { get; }

        public MainViewModel()
        {
            Content = Home;
            GoToHomeCommand = new SyncCommand(GoToHome);
            GoToHelpCommand = new SyncCommand(GoToHelp);
            GoToStartCommand = new SyncCommand(GoToStart);
            GoToAboutCommand = new SyncCommand(GoToAbout);
            GoToProjectCommand = new AsyncCommand<Project>(GoToProjectAsync, CanGoToProject);
            GoToCoreProjectCommand = new AsyncCommand<CoreProject>(GoToCoreProjectAsync, CanGoToCoreProject);
            GoToViewCommand = new AsyncCommand<View>(GoToViewAsync, CanGoToView);
            GoToCoreViewCommand = new AsyncCommand<CoreView>(GoToCoreViewAsync, CanGoToCoreView);
            CreateCoreProjectCommand = new SyncCommand<CoreProject>(CreateCoreProject);
            OpenCoreProjectCommand = new SyncCommand<CoreProject>(OpenCoreProject);
            SetUpCoreProjectCommand = new SyncCommand<CoreProject>(SetUpCoreProject);
            OpenUriCommand = new SyncCommand<string>(OpenUri);
            ExportLogsCommand = new AsyncCommand(ExportLogsAsync);
            StartEpiInfoCommand = new SyncCommand(StartEpiInfo);
            StartCommandPromptCommand = new SyncCommand(StartCommandPrompt);
        }

        private void OnError(Exception exception, string body = null)
        {
            Log.Instance.Warn(exception);
            IDialogService dialog = ServiceLocator.Resolve<IDialogService>();
            dialog.Severity = DialogSeverity.Error;
            dialog.Lead = Strings.Lead_NonFatalError;
            dialog.Body = body ?? exception.Message;
            dialog.Details = exception.ToString();
            dialog.Buttons = DialogButtonCollection.Close;
            dialog.Show();
        }

        private async Task GoToProjectAsync(Task<Project> task)
        {
            try
            {
                IProgressService progress = ServiceLocator.Resolve<IProgressService>();
                progress.Lead = Strings.Lead_LoadingProject;
                Content = await progress.Run(async () =>
                {
                    return await ProjectViewModel.CreateAsync(await task);
                });
            }
            catch (Exception ex)
            {
                OnError(ex, Strings.Body_LoadError_Project);
            }
        }

        private async Task GoToViewAsync(Task<View> task)
        {
            try
            {
                IProgressService progress = ServiceLocator.Resolve<IProgressService>();
                progress.Lead = Strings.Lead_LoadingView;
                Content = await progress.Run(async () =>
                {
                    return await ViewViewModel.CreateAsync(await task);
                });
            }
            catch (Exception ex)
            {
                OnError(ex, Strings.Body_LoadError_View);
            }
        }

        public void GoToHome()
        {
            Content = Home;
        }

        public void GoToHelp()
        {
            IWindowService window = ServiceLocator.Resolve<IWindowService>();
            window.Show(Help);
        }

        public void GoToStart()
        {
            Start.Minimized = false;
            Start.Closed = false;
        }

        public void GoToAbout()
        {
            IWindowService window = ServiceLocator.Resolve<IWindowService>();
            window.ShowDialog(About);
        }

        public bool CanGoToProject(Project project)
        {
            return project != null;
        }

        public async Task GoToProjectAsync(Project project)
        {
            await GoToProjectAsync(Task.FromResult(project));
        }

        public bool CanGoToCoreProject(CoreProject coreProject)
        {
            return Settings.Default.HasProjectPath(coreProject);
        }

        public async Task GoToCoreProjectAsync(CoreProject coreProject)
        {
            await GoToProjectAsync(Task.Run(() =>
            {
                string projectPath = Settings.Default.GetProjectPath(coreProject);
                return ProjectExtensions.Open(projectPath);
            }));
        }

        public bool CanGoToView(View view)
        {
            return view != null;
        }

        public async Task GoToViewAsync(View view)
        {
            await GoToViewAsync(Task.FromResult(view));
        }

        public bool CanGoToCoreView(CoreView coreView)
        {
            return CanGoToCoreProject(coreView.CoreProject);
        }

        public async Task GoToCoreViewAsync(CoreView coreView)
        {
            // TODO: Handle errors
            await GoToViewAsync(Task.Run(() =>
            {
                string projectPath = Settings.Default.GetProjectPath(coreView.CoreProject);
                Project project = ProjectExtensions.Open(projectPath);
                return project.Views[coreView.Name];
            }));
        }

        public void CreateCoreProject(CoreProject coreProject)
        {
            // TODO: Confirm
            WizardViewModel wizard = CreateProjectViewModels.GetWizard(coreProject);
            if (wizard.Run() != true)
            {
                return;
            }
            GoToHome();
        }

        public void OpenCoreProject(CoreProject coreProject)
        {
            // TODO: Confirm
            if (!SetUpProjectViewModels.Open(coreProject))
            {
                return;
            }
            GoToHome();
        }

        public void SetUpCoreProject(CoreProject coreProject)
        {
            // TODO: Confirm
            WizardViewModel wizard = SetUpProjectViewModels.GetWizard(coreProject);
            if (wizard.Run() != true)
            {
                return;
            }
            GoToHome();
        }

        public void OpenUri(string uri)
        {
            Process.Start(uri)?.Dispose();
        }

        public async Task ExportLogsAsync()
        {
            IFileDialogService fileDialog = ServiceLocator.Resolve<IFileDialogService>();
            fileDialog.InitialFileName = $"Logs_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            fileDialog.Filter = Strings.FileDialog_Filter_ZipFiles;
            if (fileDialog.Save() != true)
            {
                return;
            }
            IProgressService progress = ServiceLocator.Resolve<IProgressService>();
            progress.Lead = Strings.Lead_ExportingLogs;
            await progress.Run(() =>
            {
                ZipFileExtensions.CreateFromDirectory(
                    FileAppender.Directory,
                    fileDialog.FileName,
                    $"*{FileAppender.Extension}",
                    FileMode.Create,
                    FileShare.ReadWrite);
            });
        }

        public void StartEpiInfo()
        {
            Module.Menu.Start()?.Dispose();
        }

        public void StartCommandPrompt()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Environment.GetEnvironmentVariable("ComSpec")
            };
            Process.Start(startInfo)?.Dispose();
        }
    }
}
