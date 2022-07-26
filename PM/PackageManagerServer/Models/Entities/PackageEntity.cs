using PackageManagerServer.Managers;

namespace PackageManagerServer.Models.Entities
{
    public class PackageEntity
    {
        [Identity]
        public string Guid { get; set; } = "";
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Data { get; set; } = "";

    }
}
