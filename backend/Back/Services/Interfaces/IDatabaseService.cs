using System.Data.Common;
using Npgsql;

namespace Back.Services.Interfaces;

public interface IDatabaseService
{
    NpgsqlConnection GetConnection();
    DbDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command, Dictionary<string, object>? parameters = null);
    void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null);
    Task ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
    
    Task<NpgsqlDataReader> ExecuteQueryAsync(string query, Dictionary<string, object> parameters, 
        NpgsqlConnection connection, NpgsqlTransaction transaction);
    
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters, 
        NpgsqlConnection connection, NpgsqlTransaction transaction);
}
