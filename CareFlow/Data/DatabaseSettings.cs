using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace CareFlow.Data;

public static class DatabaseSettings
{
    public static string DatabasePath
    {
        get
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "CareFlow");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "careflow.db");
        }
    }

    public static string ConnectionString =>
        new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            ForeignKeys = true
        }.ToString();
}