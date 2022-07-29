using PackageManagerServer.Managers;
using PackageManagerServer.Models.Entities;

namespace PackageManagerServer.Services
{
    public static class ContextService
    {
        private static ConnectionManager? _connectionManager;
        public static ConnectionManager Connection { get { return Build(); } }

        public static ConnectionManager Build(string dbPath = "")
        {
            if (_connectionManager != null) return _connectionManager;
            _connectionManager = new ConnectionManager(dbPath);
            _connectionManager.Register<PackageEntity>();
            return _connectionManager;
        }

        private static ConnectionManager? _safelistConnection;
        public static ConnectionManager SafeListConnection { get { return BuildSafeListConnection(); } }

        public static ConnectionManager BuildSafeListConnection(string dbPath = "")
        {
            if (_safelistConnection != null) return _safelistConnection;
            _safelistConnection = new ConnectionManager(dbPath);
            _safelistConnection.Register<IdentityEntity>();
            _safelistConnection.Register<IpEntryEntity>();
            return _safelistConnection;
        }
    }
}
