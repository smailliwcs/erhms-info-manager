﻿using Epi;
using ERHMS.Common.Naming;
using ERHMS.Common.Text;
using ERHMS.Desktop.Commands;
using ERHMS.Desktop.Data;
using ERHMS.Desktop.Dialogs;
using ERHMS.Desktop.Properties;
using ERHMS.Desktop.Services;
using ERHMS.Desktop.Wizards;
using ERHMS.EpiInfo;
using ERHMS.EpiInfo.Analytics;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERHMS.Desktop.ViewModels.Wizards
{
    public abstract class CreateAssetViewModel : WizardViewModel
    {
        public class SetViewViewModel : StepViewModel<CreateAssetViewModel>
        {
            public override string Title => Strings.Lead_CreateAsset_SetView;

            public ViewCollectionView Views { get; }

            public SetViewViewModel(CreateAssetViewModel wizard)
                : base(wizard)
            {
                Views = new ViewCollectionView(wizard.Project);
            }

            public async Task InitializeAsync()
            {
                await Views.InitializeAsync();
            }

            public override bool CanContinue()
            {
                return Views.HasCurrent();
            }

            public override async Task ContinueAsync()
            {
                Wizard.View = Views.CurrentItem;
                SetFileInfoViewModel step = new SetFileInfoViewModel(Wizard, this);
                IProgressService progress = ServiceLocator.Resolve<IProgressService>();
                await progress.Run(step.InitializeAsync);
                ContinueTo(step);
            }
        }

        public class SetFileInfoViewModel : StepViewModel<CreateAssetViewModel>
        {
            public override string Title => Strings.Lead_CreateAsset_SetFileInfo;

            private FileInfo fileInfo;
            public FileInfo FileInfo
            {
                get { return fileInfo; }
                set { SetProperty(ref fileInfo, value); }
            }

            public ICommand BrowseCommand { get; }

            public SetFileInfoViewModel(CreateAssetViewModel wizard, IStep antecedent)
                : base(wizard, antecedent)
            {
                BrowseCommand = new SyncCommand(Browse);
            }

            public async Task InitializeAsync()
            {
                await Task.Run(() =>
                {
                    string fileName = $"{Wizard.View.Name}{Wizard.FileExtension}";
                    FileNameUniquifier fileNames =
                        new FileNameUniquifier(Wizard.Project.Location, Wizard.FileExtension);
                    if (fileNames.Exists(fileName))
                    {
                        fileName = fileNames.Uniquify(fileName);
                    }
                    fileInfo = new FileInfo(Path.Combine(Wizard.Project.Location, fileName));
                });
            }

            public void Browse()
            {
                IFileDialogService fileDialog = ServiceLocator.Resolve<IFileDialogService>();
                fileDialog.InitialDirectory = fileInfo.DirectoryName;
                fileDialog.InitialFileName = fileInfo.Name;
                fileDialog.Filter = Wizard.FileFilter;
                fileDialog.FileOk += FileDialog_FileOk;
                if (fileDialog.Save() != true)
                {
                    return;
                }
                FileInfo = new FileInfo(fileDialog.FileName);
            }

            private void FileDialog_FileOk(object sender, CancelEventArgs e)
            {
                IFileDialogService fileDialog = (IFileDialogService)sender;
                FileInfo fileInfo = new FileInfo(fileDialog.FileName);
                if (!Comparers.Path.Equals(fileInfo.DirectoryName, Wizard.Project.Location))
                {
                    IDialogService dialog = ServiceLocator.Resolve<IDialogService>();
                    dialog.Severity = DialogSeverity.Warning;
                    dialog.Lead = Strings.Lead_ConfirmOrphanAssetCreation;
                    dialog.Body = string.Format(Strings.Body_ConfirmOrphanAssetCreation, fileInfo.DirectoryName);
                    dialog.Buttons = DialogButtonCollection.ActionOrCancel(Strings.AccessText_Continue);
                    if (dialog.Show() != true)
                    {
                        e.Cancel = true;
                    }
                }
            }

            public override bool CanContinue()
            {
                return fileInfo != null;
            }

            public override Task ContinueAsync()
            {
                Wizard.FileInfo = fileInfo;
                ContinueTo(new CommitViewModel(Wizard, this));
                return Task.CompletedTask;
            }
        }

        public class CommitViewModel : StepViewModel<CreateAssetViewModel>
        {
            public override string Title => Strings.Lead_CreateAsset_Commit;
            public override string ContinueAction => Strings.AccessText_Finish;
            public DetailsViewModel Details { get; }

            public CommitViewModel(CreateAssetViewModel wizard, IStep antecedent)
                : base(wizard, antecedent)
            {
                Details = new DetailsViewModel
                {
                    { Strings.Label_View, wizard.View.Name },
                    { Strings.Label_FileName, wizard.FileInfo.Name },
                    { Strings.Label_DirectoryPath, wizard.FileInfo.DirectoryName }
                };
            }

            public override bool CanContinue()
            {
                return true;
            }

            public override async Task ContinueAsync()
            {
                IProgressService progress = ServiceLocator.Resolve<IProgressService>();
                progress.Lead = Strings.Lead_CreatingAsset;
                await progress.Run(() =>
                {
                    Asset asset = Wizard.CreateCore();
                    using (Stream stream = Wizard.FileInfo.Open(FileMode.Create, FileAccess.Write))
                    {
                        asset.Save(stream);
                    }
                });
                Commit();
                SetResult(true);
                ContinueTo(new CloseViewModel(Wizard, this));
            }
        }

        public class CloseViewModel : StepViewModel<CreateAssetViewModel>
        {
            public override string Title => Strings.Lead_CreateAsset_Close;
            public override string ContinueAction => Strings.AccessText_Close;

            private bool openInEpiInfo = true;
            public bool OpenInEpiInfo
            {
                get { return openInEpiInfo; }
                set { SetProperty(ref openInEpiInfo, value); }
            }

            public CloseViewModel(CreateAssetViewModel wizard, IStep antecedent)
                : base(wizard, antecedent) { }

            public override bool CanContinue()
            {
                return true;
            }

            public override async Task ContinueAsync()
            {
                Close();
                if (openInEpiInfo)
                {
                    await MainViewModel.Instance.StartEpiInfoAsync(Wizard.Module, Wizard.FileInfo.FullName);
                }
            }
        }

        protected abstract Module Module { get; }
        protected abstract string FileExtension { get; }
        protected abstract string FileFilter { get; }
        public Project Project { get; }
        public View View { get; private set; }
        public FileInfo FileInfo { get; private set; }

        protected CreateAssetViewModel(Project project)
        {
            Project = project;
        }

        public async Task InitializeAsync()
        {
            SetViewViewModel step = new SetViewViewModel(this);
            await step.InitializeAsync();
            Initialize(step);
        }

        protected abstract Asset CreateCore();
    }
}
