using System.ComponentModel.DataAnnotations;

namespace WebArchive.Models.Ldap
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "You must enter your username!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "You must enter your password!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Keep me logged in")]
        public bool RememberMe { get; set; }

        [DataType(DataType.Url)]
        public string ReturnUrl { get; set; }
    }
}
