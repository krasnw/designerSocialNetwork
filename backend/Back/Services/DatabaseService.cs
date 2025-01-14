using Npgsql;
using Back.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Back.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Build connection string with custom pooling settings
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = true,
            MinPoolSize = 1,
            MaxPoolSize = 200,
            // Remove Timeout or set it to 0 for no timeout
            CommandTimeout = 0
        };
        
        _connectionString = builder.ToString();
    }

    public NpgsqlConnection GetConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        try
        {
            connection.Open();
            using var command = new NpgsqlCommand("SET client_encoding TO 'UTF8';", connection);
            command.ExecuteNonQuery();
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection error: {ex.Message}");
            throw;
        }
    }

    public void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
    {
        using var connection = GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
        command.ExecuteNonQuery();
    }

    public NpgsqlDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command,
        Dictionary<string, object>? parameters = null)
    {
        connection = GetConnection();
        command = new NpgsqlCommand(query, connection);
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
        return command.ExecuteReader();
    }
}