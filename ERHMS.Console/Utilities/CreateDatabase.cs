﻿using ERHMS.Data;
using System;

namespace ERHMS.Console.Utilities
{
    public class CreateDatabase : IUtility
    {
        public DatabaseProvider DatabaseProvider { get; }
        public string ConnectionString { get; }

        public CreateDatabase(DatabaseProvider databaseProvider, string connectionString)
        {
            DatabaseProvider = databaseProvider;
            ConnectionString = connectionString;
        }

        public void Run()
        {
            IDatabase database = DatabaseProvider.ToDatabase(ConnectionString);
            if (database.Exists())
            {
                throw new InvalidOperationException("Database already exists.");
            }
            database.Create();
        }
    }
}
