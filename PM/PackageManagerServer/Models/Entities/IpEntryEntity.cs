using PackageManagerServer.Managers;

namespace PackageManagerServer.Models.Entities
{
    public class IpEntryEntity
    {
        [Identity]
        [AutoIncrement]
        public int Id { get; set; }
        public int IdentityId { get; set; }
        public string IpAddress { get; set; } = "";
    }
}
