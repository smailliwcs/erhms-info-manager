﻿using Epi;
using ERHMS.Desktop.Wizards;

namespace ERHMS.Desktop.ViewModels.Wizards
{
    partial class CreateProjectViewModel
    {
        public static class Blank
        {
            public class SetProjectCreationInfoViewModel : CreateProjectViewModel.SetProjectCreationInfoViewModel
            {
                public SetProjectCreationInfoViewModel(CreateProjectViewModel wizard, IStep antecedent)
                    : base(wizard, antecedent) { }

                protected override void GoToNextStep()
                {
                    GoToStep(new CommitViewModel(Wizard, this));
                }
            }

            public class CommitViewModel : CreateProjectViewModel.CommitViewModel
            {
                public CommitViewModel(CreateProjectViewModel wizard, IStep antecedent)
                    : base(wizard, antecedent) { }

                protected override void ContinueCore(Project project) { }
            }
        }
    }
}
