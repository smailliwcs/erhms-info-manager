﻿using Epi;
using ERHMS.Common.Linq;
using ERHMS.Common.Logging;
using ERHMS.EpiInfo;
using ERHMS.EpiInfo.Data;
using System.IO;

namespace ERHMS.Console.Utilities
{
    public class ImportRecords : IUtility
    {
        public string ProjectPath { get; }
        public string ViewName { get; }
        public string InputPath { get; }

        public ImportRecords(string projectPath, string viewName, string inputPath)
        {
            ProjectPath = projectPath;
            ViewName = viewName;
            InputPath = inputPath;
        }

        public void Run()
        {
            Project project = ProjectExtensions.Open(ProjectPath);
            View view = project.Views[ViewName];
            using (Stream stream = File.Open(InputPath, FileMode.Open, FileAccess.Read))
            using (TextReader reader = new StreamReader(stream))
            {
                RecordImporter importer = new RecordImporter(view, reader);
                foreach (Iterator<string> header in importer.Headers.Iterate())
                {
                    if (view.Fields.DataFields.Contains(header.Value))
                    {
                        importer.MapField(header.Index, view.Fields[header.Value]);
                    }
                }
                importer.Import();
                foreach (string error in importer.Errors)
                {
                    Log.Instance.Warn(error);
                }
            }
        }
    }
}
