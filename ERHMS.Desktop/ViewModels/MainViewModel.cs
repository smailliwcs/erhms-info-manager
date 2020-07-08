﻿using ERHMS.Desktop.Commands;
using ERHMS.Utility;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace ERHMS.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object content = new HomeViewModel();
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                Log.Default.Debug($"Displaying {value}");
                SetProperty(ref content, value);
            }
        }

        public ICommand GoHomeCommand { get; }
        public ICommand OpenEpiInfoCommand { get; }
        public ICommand OpenFileExplorerCommand { get; }

        public MainViewModel()
        {
            GoHomeCommand = new Command(GoHome);
            OpenEpiInfoCommand = new Command(OpenEpiInfo);
            OpenFileExplorerCommand = new Command(OpenFileExplorer);
        }

        private void GoHome()
        {
            Content = new HomeViewModel();
        }

        private void OpenEpiInfo()
        {
            string entryDirectory = ReflectionExtensions.GetEntryDirectory();
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = entryDirectory,
                FileName = Path.Combine(entryDirectory, "EpiInfo.exe")
            });
        }

        private void OpenFileExplorer()
        {
            Process.Start(ReflectionExtensions.GetEntryDirectory());
        }
    }
}
