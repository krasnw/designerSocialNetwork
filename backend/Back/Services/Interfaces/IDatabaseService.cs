using Npgsql;

namespace Back.Services.Interfaces;

public interface IDatabaseService
{
    NpgsqlConnection GetConnection();
    void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null);
    NpgsqlDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command,
        Dictionary<string, object>? parameters = null);
}
