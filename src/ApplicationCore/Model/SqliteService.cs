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
    private bool _isInitialized;
    private readonly string _connectionString;
    private readonly string _dbPath;

    public SqliteService(string appDataPath) {
        _dbPath = Path.Combine(appDataPath, "database.sqlite");
        _connectionString = $"Data Source={_dbPath}";
    }

    /// <summary>
    /// Creates database if it does not exist
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task InitializeAsync() {
        if (_isInitialized) return;

        if (File.Exists(_dbPath)) {
            // Database already exists, no need to create it again
            // suggestion: maybe check if the file has the correct schema?
            // if not: drop the database and create a new one
            _isInitialized = true;
            return;
        }

        #region Get schema
        // string schemaFilePath = Path.Combine(AppContext.BaseDirectory, "Schemata", "database.sql");
        // if (!File.Exists(schemaFilePath))
        // {
        //     throw new FileNotFoundException("Schema file not found", schemaFilePath);
        // }

        // string schemaSql = await File.ReadAllTextAsync(schemaFilePath);

        var asm = typeof(SqliteService).Assembly;
        using var stream = asm.GetManifestResourceStream("ApplicationCore.Schemata.database.sql");
        using var reader = new StreamReader(stream!);
        string schemaSql = reader.ReadToEnd();
        #endregion

        #region Create database
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = schemaSql;
        await command.ExecuteNonQueryAsync();
        #endregion

        _isInitialized = true;
    }

    /// <summary>
    /// Throws an exception if <see cref="InitializeAsync()"/> was not called
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void ThrowIfNotInitialized() {
        if (_isInitialized) return;

        throw new InvalidOperationException("Database Controller not initialized. Call InitializeAsync() first.");
    }

    /// <summary>
    /// Execute INSERT/UPDATE/DELETE asynchronously
    /// </summary>
    /// <param name="commandText">sql command</param>
    /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
    public async Task<int> NonQueryAsync(string commandText, IDictionary<string, object>? parameters = null) {
        ThrowIfNotInitialized();

        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        AddParametersToCommand(command, parameters);

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute SELECT asynchronously
    /// </summary>
    /// <param name="queryText">sql command</param>
    /// <returns>DbDataReader (needs to be disposed of after use)</returns>
    public async Task<DbDataReader> QueryAsync(string queryText, IDictionary<string, object>? parameters = null) {
        ThrowIfNotInitialized();
        
        SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = queryText;
        AddParametersToCommand(command, parameters);
        
        // CommandBehavior.CloseConnection: automatically close the db connection, when DbDataReader is disposed
        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }

    private static void AddParametersToCommand(SqliteCommand command, IDictionary<string, object>? parameters) {
        if (parameters == null) return;

        foreach (KeyValuePair<string, object> parameter in parameters) {
            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
        }
    }
}