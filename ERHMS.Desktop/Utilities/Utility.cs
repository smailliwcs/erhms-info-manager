﻿using ERHMS.Common;
using ERHMS.Common.Linq;
using ERHMS.Common.Logging;
using ERHMS.Common.Reflection;
using ERHMS.Desktop.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ERHMS.Desktop.Utilities
{
    public abstract class Utility : IUtility
    {
        public abstract class Headless : Utility
        {
            public override void Invoke()
            {
                using (Process process = GetProcess())
                {
                    Output = process.StandardOutput.ReadToEnd();
                }
            }
        }

        public abstract class Graphical : Utility
        {
            private static void OnError(Exception exception)
            {
                Log.Instance.Error(exception);
                MessageBox.Show(
                    string.Format(Strings.Body_UtilityError, exception.Message),
                    Strings.App_Title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            protected virtual string Lead => Strings.Lead_Working;
            protected virtual string Body => "";

            private UtilityDialog GetDialog()
            {
                return new UtilityDialog
                {
                    Owner = Form.ActiveForm,
                    StartPosition = FormStartPosition.CenterParent,
                    Lead = Lead,
                    Body = Body
                };
            }

            public override void Invoke()
            {
                try
                {
                    using (Process process = GetProcess())
                    using (UtilityDialog dialog = GetDialog())
                    using (BackgroundWorker worker = new BackgroundWorker())
                    {
                        worker.DoWork += (sender, e) =>
                        {
                            process.Start();
                            Output = process.StandardOutput.ReadToEnd();
                        };
                        worker.RunWorkerCompleted += (sender, e) =>
                        {
                            if (e.Error != null)
                            {
                                OnError(e.Error);
                            }
                            dialog.Done = true;
                            dialog.Close();
                        };
                        dialog.Shown += (sender, e) =>
                        {
                            worker.RunWorkerAsync();
                        };
                        if (dialog.ShowDialog() == DialogResult.Cancel)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        private static readonly IEnumerable<Type> instanceTypes = typeof(IUtility).GetInstanceTypes().ToList();

        private static Type GetInstanceType(string instanceTypeName)
        {
            foreach (Type instanceType in instanceTypes)
            {
                if (instanceType.Name == instanceTypeName)
                {
                    return instanceType;
                }
            }
            throw new ArgumentException($"Utility '{instanceTypeName}' does not exist.");
        }

        public static async Task ExecuteAsync(string[] args)
        {
            Type instanceType = GetInstanceType(args[0]);
            Log.Instance.Debug($"Executing utility: {instanceType.Name}");
            IUtility utility = (IUtility)Activator.CreateInstance(instanceType);
            utility.Parameters = args.Skip(1);
            await utility.ExecuteAsync();
        }

        public virtual IEnumerable<string> Parameters { get; set; }
        protected string Output { get; private set; }

        private Process GetProcess()
        {
            string workingDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            IEnumerable<string> args = Parameters.Prepend(GetType().Name);
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectoryPath,
                    FileName = Path.Combine(workingDirectoryPath, "ERHMS Info Manager.exe"),
                    Arguments = CommandLine.Quote(args),
                    RedirectStandardOutput = true
                }
            };
        }

        public abstract void Invoke();
        public abstract Task ExecuteAsync();
    }
}
