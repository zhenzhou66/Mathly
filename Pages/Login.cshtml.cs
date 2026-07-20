using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages
{
    public class LoginModel : PageModel
    {
        private readonly MathlyDbContext _db;

        public LoginModel(MathlyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string UserID { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter both User ID and Password.";
                return Page();
            }

            // find user in database
            var user = _db.LoginCredentials
                .FirstOrDefault(u => u.UserID == UserID && u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Invalid User ID or Password.";
                return Page();
            }

            // Build the identity that gets stored in the auth cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = RememberMe,                        // "Remember me" checkbox controls whether the cookie survives browser close
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return user.Role.ToLower() switch
            {
                "student" => RedirectToPage("/Student/Dashboard"),
                "teacher" => RedirectToPage("/Teacher/Dashboard"),
                "admin" => RedirectToPage("/Landing"), // TODO: point to an Admin dashboard once one exists
                _ => RedirectToPage("/Landing")
            };
        }
    }
}