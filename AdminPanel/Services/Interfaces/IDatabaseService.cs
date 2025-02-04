namespace AdminPanel.Services.Interfaces;
using System.Data;
using Npgsql;

public interface IDatabaseService
{
    Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<T> ExecuteWithConnectionAsync<T>(Func<NpgsqlConnection, Task<T>> operation);
}
