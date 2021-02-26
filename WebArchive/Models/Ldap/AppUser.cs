using WebArchive.Interfaces;

namespace WebArchive.Models.Ldap
{
    public class AppUser : IAppUser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string BadgeType { get; set; }
        public string BadgeId { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string Fistname { get; set; }
        public string Lastname { get; set; }
        public string Initials { get; set; }
        public string Domain { get; set; }
        public byte[] Thumbnail { get; set; }
        public string[] Roles { get; set; }
    }
}
