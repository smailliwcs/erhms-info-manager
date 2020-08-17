﻿using ERHMS.Desktop.Infrastructure;
using ERHMS.Desktop.Views;
using ERHMS.Desktop.Wizards;
using System.Windows;

namespace ERHMS.Desktop.Services
{
    internal class WizardService : IWizardService
    {
        private readonly Application application;

        public WizardService(Application application)
        {
            this.application = application;
        }

        public bool? Run(IWizard wizard)
        {
            if (application.Dispatcher.CheckAccess())
            {
                return RunInternal(wizard);
            }
            else
            {
                return application.Dispatcher.Invoke(() => RunInternal(wizard));
            }
        }

        private bool? RunInternal(IWizard wizard)
        {
            Window owner = application.GetActiveWindow();
            Window window = new WizardView
            {
                Owner = owner,
                DataContext = wizard
            };
            window.ShowDialog();
            return wizard.Result;
        }
    }
}
