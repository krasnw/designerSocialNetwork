using Npgsql;

namespace Back.Services
{
    public class DatabaseService : IDisposable
    {
        private static DatabaseService? _instance;
        private static readonly object Lock = new();
        private readonly NpgsqlConnection _connection;

        private DatabaseService(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public static DatabaseService GetInstance(string? connectionString = null)
        {
            if (_instance != null) return _instance;
            if (connectionString == null) throw new Exception("No connection string provided, but no instance exists.");
            lock (Lock)
            {
                _instance ??= new DatabaseService(connectionString);
            }
            return _instance;
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed || _connection.State == System.Data.ConnectionState.Broken)
            {
                _connection.Open();
            }
            return _connection;
        }

        public void Dispose()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
            _connection.Dispose();
        }

        public void ExecuteNonQuery(string query)
        {
            try
            {
                using var command = new NpgsqlCommand(query, GetConnection());
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing non-query: {ex.Message}");
                throw;
            }
        }

        public NpgsqlDataReader ExecuteQuery(string query)
        {
            try
            {
                using var command = new NpgsqlCommand(query, GetConnection());
                command.Transaction = _connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing query: {ex.Message}");
                throw;
            }
        }
    }
}