
namespace WebArchive.Models.Ldap
{
    public class CookiesConfig
    {
        public string CookieName { get; set; }
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }
        public string AccessDeniedPath { get; set; }
        public string ReturnUrlParameter { get; set; }
    }
}
