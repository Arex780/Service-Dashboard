using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebArchive.Interfaces;
using WebArchive.Models.Ldap;

namespace WebArchive.Controllers
{
    public class AccountController : Controller
    {
        private readonly Interfaces.IAuthenticationService Auth;

        public AccountController(Interfaces.IAuthenticationService authService)
        {
            Auth = authService;
        }

        [Route("account/login")]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = Auth.Login(model.Username, model.Password);

                    // If the user is authenticated, store its claims to cookie
                    if (user != null)
                    {
                        var userClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.AuthorizationDecision, user.BadgeType),
                            new Claim(ClaimTypes.NameIdentifier, user.BadgeId),
                            new Claim(ClaimTypes.WindowsAccountName, user.DisplayName),
                            new Claim(ClaimTypes.Dns, user.Domain),
                            new Claim(ClaimTypes.Thumbprint, Convert.ToBase64String(user.Thumbnail)),
                            new Claim(ClaimTypes.X500DistinguishedName, user.DistinguishedName),
                            new Claim(ClaimTypes.GivenName, user.Fistname),
                            new Claim(ClaimTypes.Surname, user.Lastname),
                            new Claim(ClaimTypes.WindowsUserClaim, user.Initials)
                        };

                        // Roles
                        foreach (var role in user.Roles)
                        {
                            userClaims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(userClaims, Auth.GetType().Name)
                        );

                        await HttpContext.SignInAsync(
                          CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = model.RememberMe
                            }
                        );

                        return Redirect(Url.IsLocalUrl(model.ReturnUrl)
                            ? model.ReturnUrl
                            : "/");
                    }

                    ModelState.AddModelError("", @"Your username or password
                        is incorrect. Please try again.");
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Referral")
                        ModelState.AddModelError("", @"Invalid Username");
                    else
                        ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        [Route("account/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/account/login");
        }

        [Route("account/accessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("account")]
        public IActionResult Index()
        {
            return View();
        }

    }
}