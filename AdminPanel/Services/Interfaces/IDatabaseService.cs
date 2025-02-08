namespace AdminPanel.Services.Interfaces;
using System.Data;
using Npgsql;
using System.Data.Common;

public interface IDatabaseService
{
    Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<T> ExecuteWithConnectionAsync<T>(Func<NpgsqlConnection, Task<T>> operation);
    Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null);
}
