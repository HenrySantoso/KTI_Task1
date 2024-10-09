using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.ViewModels;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _userData;

        public AccountController(IUser userData)
        {
            _userData = userData;
        }

        // GET: AccountController
        public ActionResult Index()
        {
            var users = _userData.GetUsers();
            return View(users);
        }

        public ActionResult Register()
        {
            return View();
        }

            [HttpPost]
        public IActionResult Register(RegistrationViewModel registrationViewModel)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var user = new User
                    {
                        Username = registrationViewModel.Username,
                        Password = registrationViewModel.Password,
                        RoleName = "contributor"
                    };
                    _userData.Registration(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(registrationViewModel);
        }

        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                    var user = new User
                    {
                        Username = loginViewModel.Username,
                        Password = loginViewModel.Password
                    };
                    
                    var loginUser = _userData.Login(user);
                    if(loginUser == null)
                    {
                        ViewBag.Message = "Invalid login attempt";
                        return View(loginViewModel);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username)
                    };

                    var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = loginViewModel.RememberLogin
                        }
                    );
                    return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(loginViewModel);
        }
    }
}
