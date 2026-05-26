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

        public string ErrorMessage { get; set; }

        public void OnGet() { }
        public IActionResult OnPost()
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

            ErrorMessage = "Login successful!";
            return Page();
        }
    }
}