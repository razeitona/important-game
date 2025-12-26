using System.Data;

namespace important_game.infrastructure.Data.Connections
{
    /// <summary>
    /// Factory abstraction for database connections.
    /// Implements the Factory pattern to create and manage database connections.
    /// This follows the SOLID principle of Dependency Inversion.
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates and returns a new database connection.
        /// </summary>
        /// <returns>A new IDbConnection instance ready to use</returns>
        IDbConnection CreateConnection();
    }
}
