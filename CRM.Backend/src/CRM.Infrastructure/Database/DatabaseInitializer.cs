// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CRM.Infrastructure.Database;

/// <summary>
/// Supported database providers
/// </summary>
public enum DatabaseProvider
{
    SQLite,
    SqlServer,
    PostgreSQL,
    MySQL,
    MariaDB,
    Oracle
}

/// <summary>
/// Database initializer that creates the database schema based on the configured provider
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current database provider from configuration
    /// </summary>
    public DatabaseProvider GetDatabaseProvider()
    {
        var providerName = _configuration["DatabaseProvider"]?.ToLowerInvariant() ?? "mariadb";

        return providerName switch
        {
            "sqlserver" or "mssql" => DatabaseProvider.SqlServer,
            "postgresql" or "postgres" or "npgsql" => DatabaseProvider.PostgreSQL,
            "mysql" => DatabaseProvider.MySQL,
            "mariadb" => DatabaseProvider.MariaDB,
            "oracle" => DatabaseProvider.Oracle,
            _ => DatabaseProvider.MariaDB
        };
    }

    /// <summary>
    /// Gets the SQL script for creating the database schema
    /// </summary>
    public string GetCreateDatabaseScript()
    {
        var provider = GetDatabaseProvider();
        return GetScriptForProvider(provider, "001_CreateDatabase.sql");
    }

    /// <summary>
    /// Gets a specific SQL script for the current database provider
    /// </summary>
    public string GetScriptForProvider(DatabaseProvider provider, string scriptName)
    {
        var providerFolder = provider switch
        {
            DatabaseProvider.SqlServer => "SqlServer",
            DatabaseProvider.PostgreSQL => "PostgreSQL",
            DatabaseProvider.MySQL or DatabaseProvider.MariaDB => "MySQL",
            DatabaseProvider.Oracle => "Oracle",
            _ => "SQLite"
        };

        var scriptPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
            "Database", "Scripts", providerFolder, scriptName);

        // Try embedded resource first
        var resourceName = $"CRM.Infrastructure.Database.Scripts.{providerFolder}.{scriptName}";
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        // Fall back to file system
        if (File.Exists(scriptPath))
        {
            return File.ReadAllText(scriptPath);
        }

        throw new FileNotFoundException($"Database script not found: {scriptName} for provider {provider}");
    }

    /// <summary>
    /// Validates that the connection string is properly configured
    /// </summary>
    public bool ValidateConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("Connection string 'DefaultConnection' is not configured");
            return false;
        }

        var provider = GetDatabaseProvider();
        _logger.LogInformation("Database provider: {Provider}", provider);
        _logger.LogInformation("Connection string configured: {Length} characters", connectionString.Length);

        return true;
    }

    /// <summary>
    /// Gets connection string examples for each provider
    /// </summary>
    public static Dictionary<DatabaseProvider, string> GetConnectionStringExamples()
    {
        return new Dictionary<DatabaseProvider, string>
        {
            [DatabaseProvider.SQLite] = "Data Source=crm.db",
            [DatabaseProvider.SqlServer] = "Server=localhost;Database=CrmDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",
            [DatabaseProvider.PostgreSQL] = "Host=localhost;Port=5432;Database=crm_database;Username=postgres;Password=YourPassword;",
            [DatabaseProvider.MySQL] = "Server=localhost;Port=3306;Database=crm_database;User=root;Password=YourPassword;",
            [DatabaseProvider.MariaDB] = "Server=localhost;Port=3306;Database=crm_database;User=root;Password=YourPassword;",
            [DatabaseProvider.Oracle] = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=crm;Password=YourPassword;"
        };
    }

    /// <summary>
    /// Gets the NuGet package required for each provider
    /// </summary>
    public static Dictionary<DatabaseProvider, string> GetRequiredPackages()
    {
        return new Dictionary<DatabaseProvider, string>
        {
            [DatabaseProvider.SQLite] = "Microsoft.EntityFrameworkCore.Sqlite",
            [DatabaseProvider.SqlServer] = "Microsoft.EntityFrameworkCore.SqlServer",
            [DatabaseProvider.PostgreSQL] = "Npgsql.EntityFrameworkCore.PostgreSQL",
            [DatabaseProvider.MySQL] = "Pomelo.EntityFrameworkCore.MySql",
            [DatabaseProvider.MariaDB] = "Pomelo.EntityFrameworkCore.MySql",
            [DatabaseProvider.Oracle] = "Oracle.EntityFrameworkCore"
        };
    }
}
