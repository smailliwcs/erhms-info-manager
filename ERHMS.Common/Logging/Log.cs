﻿using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

namespace ERHMS.Common.Logging
{
    public static class Log
    {
        private class ProgressImpl : IProgress<string>
        {
            public void Report(string value)
            {
                Instance.Debug(value);
            }
        }

        public static ILog Instance => LogManager.GetLogger(typeof(Log));
        public static IProgress<string> Progress { get; } = new ProgressImpl();

        static Log()
        {
            try
            {
                using (WindowsIdentity user = WindowsIdentity.GetCurrent())
                {
                    GlobalContext.Properties["user"] = user.Name;
                }
            }
            catch { }
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    GlobalContext.Properties["process"] = process.Id;
                }
            }
            catch { }
        }

        public static void Initialize(params IAppender[] appenders)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            foreach (IAppender appender in appenders)
            {
                if (appender is IOptionHandler optionHandler)
                {
                    optionHandler.ActivateOptions();
                }
                hierarchy.Root.AddAppender(appender);
            }
            hierarchy.Configured = true;
        }

        public static string GetFile(this ILog @this)
        {
            return @this.Logger.Repository.GetAppenders()
                .OfType<FileAppender>()
                .FirstOrDefault()
                ?.File;
        }
    }
}
