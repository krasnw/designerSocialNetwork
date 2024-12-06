using Npgsql;
using Back.Services.Interfaces;

namespace Back.Services
{
    public class DatabaseService : IDatabaseService
    {
        private static DatabaseService? _instance;
        private static readonly object Lock = new();
        private readonly string _connectionString;

        private DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static DatabaseService GetInstance(string? connectionString = null)
        {
            if (_instance != null) return _instance;
            if (connectionString == null) throw new Exception("No connection string provided, but no instance exists.");
            lock (Lock)
            {
                _instance = new DatabaseService(connectionString);
            }

            return _instance;
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open(); // Ensure the connection is opened
            return connection;
        }

        public void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing non-query: {ex.Message}");
                throw;
            }
        }

        public NpgsqlDataReader ExecuteQuery(string query, out NpgsqlConnection connection, out NpgsqlCommand command,
            Dictionary<string, object>? parameters = null)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing query: {ex.Message}");
                throw;
            }
        }
    }
}