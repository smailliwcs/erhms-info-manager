﻿using System;
using System.Threading.Tasks;

namespace ERHMS.Desktop.Commands
{
    public class SyncCommand : Command
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public SyncCommand(Action execute, Func<bool> canExecute = null, ErrorBehavior errorBehavior = ErrorBehavior.Raise)
            : base(execute, errorBehavior)
        {
            this.execute = execute;
            this.canExecute = canExecute ?? Always;
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public override Task ExecuteCore(object parameter)
        {
            execute();
            return Task.CompletedTask;
        }
    }

    public class SyncCommand<T> : Command
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public SyncCommand(Action<T> execute, Func<T, bool> canExecute = null, ErrorBehavior errorBehavior = ErrorBehavior.Raise)
            : base(execute, errorBehavior)
        {
            this.execute = execute;
            this.canExecute = canExecute ?? Always;
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute((T)parameter);
        }

        public override Task ExecuteCore(object parameter)
        {
            execute((T)parameter);
            return Task.CompletedTask;
        }
    }
}
