using Npgsql;

namespace Back.Services.Interfaces;

public interface IDatabaseService
{
    NpgsqlConnection GetConnection();
    NpgsqlDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command, Dictionary<string, object>? parameters = null);
    void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null);
    Task ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
}
