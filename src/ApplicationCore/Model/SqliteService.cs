using System.Data;
using System.Data.Common;
using ApplicationCore.Interfaces;
using Microsoft.Data.Sqlite;

namespace ApplicationCore.Model;

/// <summary>
/// light-weight SqliteService
/// <para>
/// <see cref="InitializeAsync" /> must be called once before using the other methods
/// </para>
/// </summary>
public class SqliteService : IDatabaseService {
    private readonly string _connectionString;
    private readonly string _dbPath;

    public SqliteService(string? dbPath = null) {
        string defaultDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "database.sqlite");
        _dbPath = string.IsNullOrWhiteSpace(dbPath) ? defaultDbPath : dbPath;
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync() {
        // create database if it does not exist
        if (File.Exists(_dbPath)) return;

        string schemaFilePath = Path.Combine(AppContext.BaseDirectory, "Database", "Scripts", "schema.sql");
        if (!File.Exists(schemaFilePath)) {
            throw new FileNotFoundException("Schema file not found", schemaFilePath);
        }

        string schemaSql = await File.ReadAllTextAsync(schemaFilePath);

        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = schemaSql;
        await command.ExecuteNonQueryAsync();
    }


    /// <summary>
    /// Execute INSERT/UPDATE/DELETE asynchronously
    /// </summary>
    /// <param name="commandText">sql command</param>
    /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
    public async Task<int> NonQueryAsync(string commandText) {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute SELECT asynchronously
    /// </summary>
    /// <param name="queryText"></param>
    /// <returns>DbDataReader (needs to be disposed of after use)</returns>
    public async Task<DbDataReader> QueryAsync(string queryText) {
        SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = queryText;
        // CommandBehavior.CloseConnection: automatically close the db connection, when DbDataReader is disposed
        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }
}