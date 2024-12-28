using Npgsql;
using Back.Services.Interfaces;

namespace Back.Services;

public class DatabaseService : IDatabaseService
{
    private static DatabaseService? _instance;
    private static readonly object Lock = new();
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static DatabaseService GetInstance(string? connectionString = null)
    {
        if (_instance != null) return _instance;
        if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString),
                "Connection string is required when creating new instance.");
        lock (Lock)
        {
            _instance ??= new DatabaseService(connectionString);
        }

        return _instance;
    }

    public NpgsqlConnection GetConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        // Set UTF-8 encoding for the connection
        using var command = new NpgsqlCommand("SET client_encoding TO 'UTF8';", connection);
        command.ExecuteNonQuery();
        return connection;
    }

    private void AddParameters(NpgsqlCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
    }

    public void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
    {
        using var connection = GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        AddParameters(command, parameters);
        command.ExecuteNonQuery();
    }

    public NpgsqlDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command,
        Dictionary<string, object>? parameters = null)
    {
        connection = GetConnection();
        command = new NpgsqlCommand(query, connection);
        AddParameters(command, parameters);
        return command.ExecuteReader();
    }
}