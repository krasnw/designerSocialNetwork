using System.Data;
using System.Data.Common;
using Npgsql;
using Back.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Back.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _logger = logger;
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Adjust pooling settings for high-load scenarios
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = true,
            MinPoolSize = 10,
            MaxPoolSize = 100,
            ConnectionIdleLifetime = 300,
            ConnectionPruningInterval = 10,
            Timeout = 30,
            CommandTimeout = 30,
            KeepAlive = 30,
            ConnectionLifetime = 1800 // 30 minutes
        };

        _connectionString = builder.ToString();
        _logger.LogInformation("Database connection configured with MaxPoolSize: {MaxPoolSize}, Timeout: {Timeout}s",
            builder.MaxPoolSize, builder.Timeout);
    }

    public NpgsqlConnection GetConnection()
    {
        try
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("SET client_encoding = 'UTF8';", connection);
            command.ExecuteNonQuery();
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError("Database connection error: {Message}", ex.Message);
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

    public async Task ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
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
        await command.ExecuteNonQueryAsync();
    }

    public DbDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command,
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

    // Modify ExecuteQueryAsync to properly dispose connections
    public async Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        var connection = GetConnection();
        try
        {
            var command = new NpgsqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
            // Important: Use CommandBehavior.CloseConnection to ensure the connection is closed when the reader is disposed
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object> parameters,
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var command = new NpgsqlCommand(query, connection, transaction);
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
        return await command.ExecuteReaderAsync();
    }

    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters,
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var command = new NpgsqlCommand(query, connection, transaction);
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
        return await command.ExecuteNonQueryAsync();
    }
}