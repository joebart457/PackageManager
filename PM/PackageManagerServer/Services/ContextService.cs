using PackageManagerServer.Managers;
using PackageManagerServer.Models.Entities;

namespace PackageManagerServer.Services
{
    public static class ContextService
    {
        private static ConnectionManager _connectionManager;
        public static ConnectionManager Connection { get { return _connectionManager; } }

        public static void CreateContext(string dbPath = "")
        {
            _connectionManager = new ConnectionManager(dbPath);
            _connectionManager.Register<PackageEntity>();
        }
    }
}
