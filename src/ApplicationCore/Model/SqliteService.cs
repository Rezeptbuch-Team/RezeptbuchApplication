using ApplicationCore.Interfaces;

using Microsoft.Data.Sqlite;

namespace ApplicationCore.Model;

public class SqliteService : IDatabaseService {
    private readonly string connectionString;

    public SqliteService() {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "database.sqlite");
        connectionString = $"Data Source={dbPath}";

        string schemaFilePath = Path.Combine(AppContext.BaseDirectory, "Database", "Scripts", "schema.sql");
        if (!File.Exists(dbPath)) {
            CreateDatabaseFromSchema(schemaFilePath);
        }
    }

    private void CreateDatabaseFromSchema(string schemaFilePath) {
        if (!File.Exists(schemaFilePath)) {
            throw new FileNotFoundException("Schema file not found", schemaFilePath);
        }

        string schemaSql = File.ReadAllText(schemaFilePath);

        using SqliteConnection connection = new(connectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = schemaSql;
        command.ExecuteNonQuery();
    }
}