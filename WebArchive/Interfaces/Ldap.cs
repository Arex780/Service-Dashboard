
namespace WebArchive.Interfaces
{
    public interface IAppUser
    {
        string Username { get; }
        string Email { get; }
        string BadgeType { get; }
        string BadgeId { get; }
        string DisplayName { get; }
        string DistinguishedName { get; }
        string Fistname { get; }
        string Lastname { get; }
        string Initials { get; }
        string Domain { get; }
        byte[] Thumbnail { get; }
        string[] Roles { get; }
    }

    public interface IAuthenticationService
    {
        IAppUser Login(string username, string password);
    }
}
