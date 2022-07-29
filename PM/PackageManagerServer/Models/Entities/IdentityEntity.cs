using PackageManagerServer.Managers;

namespace PackageManagerServer.Models.Entities
{
    public class IdentityEntity
    {
        [Identity]
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public bool IsLocked { get; set; }

    }
}
