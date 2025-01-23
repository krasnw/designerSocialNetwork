using Npgsql;

namespace Back.Tests.Helpers
{
    public static class TestDatabaseHelper
    {
        public static async Task<bool> IsTestDatabaseAvailable(string connectionString)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task CreateTestDatabase(string connectionString)
        {
            // Implementation for setting up test database
            // This would create necessary tables and test data
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Add test setup SQL here
            const string createTestTable = @"
                CREATE TABLE IF NOT EXISTS test_table (
                    id SERIAL PRIMARY KEY,
                    name TEXT NOT NULL
                );";

            using var cmd = new NpgsqlCommand(createTestTable, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task CleanupTestDatabase(string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Add cleanup SQL here
            const string dropTestTable = "DROP TABLE IF EXISTS test_table;";
            
            using var cmd = new NpgsqlCommand(dropTestTable, connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
