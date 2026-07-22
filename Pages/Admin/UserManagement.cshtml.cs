using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Admin
{
    public class UserManagementModel : PageModel
    {
        private readonly MathlyDbContext _db;
        public UserManagementModel(MathlyDbContext db) => _db = db;

        public List<UserRow> Users { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class UserRow
        {
            public string UserID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
            public string StudyLevel { get; set; }
            public string School { get; set; }
            public int Age { get; set; }
            public DateOnly BirthDate { get; set; }
            public DateOnly DateJoined { get; set; }
            public int ExpPoints { get; set; }
            public string Status { get; set; }
        }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var logins = await _db.LoginCredentials.ToDictionaryAsync(l => l.UserID, l => l.Status);
            var students = await _db.Students.ToListAsync();

            Users = students.Select(s => new UserRow
            {
                UserID = s.UserID,
                Name = s.StudentName ?? s.UserID,
                Email = s.Email ?? "",
                PhoneNumber = s.PhoneNumber ?? "",
                Role = "student",
                StudyLevel = s.StudyLevel,
                School = s.School,
                Age = s.StudentAge,
                BirthDate = s.BirthDate,
                DateJoined = s.DateJoined,
                ExpPoints = s.ExpPoints,
                Status = logins.GetValueOrDefault(s.UserID, "active")
            })
            .OrderBy(u => u.UserID)
            .ThenBy(u => u.Name)
            .ToList();
        }

        public async Task<IActionResult> OnPostSuspendAsync(string userId)
        {
            var login = await _db.LoginCredentials.FindAsync(userId);
            if (login != null)
            {
                login.Status = "suspended";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' suspended.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(string userId)
        {
            var login = await _db.LoginCredentials.FindAsync(userId);
            if (login != null)
            {
                login.Status = "active";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' restored.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            var login = await _db.LoginCredentials.FindAsync(userId);
            if (login == null || login.Status != "suspended")
            {
                TempData["Error"] = "Only suspended users can be deleted.";
                return RedirectToPage();
            }

            var student = await _db.Students.FindAsync(userId);
            if (student != null) _db.Students.Remove(student);

            _db.LoginCredentials.Remove(login);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{userId}' deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(
            string userId, string name, string email, string phoneNumber, string level, string school)
        {
            var student = await _db.Students.FindAsync(userId);
            if (student != null)
            {
                student.StudentName = name;
                student.Email = email;
                student.PhoneNumber = phoneNumber;
                student.StudyLevel = level;
                student.School = school;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' updated.";
            }
            return RedirectToPage();
        }
    }
}