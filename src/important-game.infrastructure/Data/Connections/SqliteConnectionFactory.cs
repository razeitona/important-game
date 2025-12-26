using System.Data;
using System.Data.SQLite;

namespace important_game.infrastructure.Data.Connections
{
    /// <summary>
    /// SQLite implementation of IDbConnectionFactory.
    /// Creates SQLite connections from a connection string.
    /// </summary>
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty");

            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}
