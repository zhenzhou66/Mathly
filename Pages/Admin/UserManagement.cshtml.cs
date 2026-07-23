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
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class UserRow
        {
            public string UserID { get; set; } = "";
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public string Role { get; set; } = "";
            public string Level { get; set; } = "";      
            public string? StudyLevel { get; set; } 
            public string? School { get; set; }
            public int? Age { get; set; }
            public string? TopicName { get; set; }
            public int? ExpPoints { get; set; }
            public DateOnly? BirthDate { get; set; }
            public DateOnly DateJoined { get; set; }
            public string Status { get; set; } = "";
            public string Password { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var logins = await _db.LoginCredentials.ToListAsync();
            var students = await _db.Students.ToListAsync();
            var teachers = await _db.Teachers.ToListAsync();
            var admins = await _db.Admins.ToListAsync();
            var topicsDict = await _db.Topics.ToDictionaryAsync(t => t.TopicID, t => t.TopicName);

            Users = logins.Select(l =>
            {
                var s = students.FirstOrDefault(x => x.UserID == l.UserID);
                var t = teachers.FirstOrDefault(x => x.UserID == l.UserID);
                var a = admins.FirstOrDefault(x => x.UserID == l.UserID);

                var teacherTopic = t != null && !string.IsNullOrEmpty(t.TopicID)
                    ? topicsDict.GetValueOrDefault(t.TopicID)
                    : null;

                return new UserRow
                {
                    UserID = l.UserID,
                    Name = s?.StudentName ?? t?.TeacherName ?? a?.AdminName ?? l.UserID,
                    Email = s?.Email ?? t?.Email ?? a?.Email ?? "",
                    PhoneNumber = s?.PhoneNumber ?? t?.PhoneNumber ?? a?.PhoneNumber ?? "",
                    Role = l.Role,
                    // Students show "Form X · School"; teachers show their topic; admins show "—"
                    Level = s != null ? $"{s.StudyLevel} · {s.School}"
                          : t != null ? (teacherTopic ?? "—")
                          : "—",
                    StudyLevel = s?.StudyLevel,
                    School = s?.School,
                    Age = s?.StudentAge,
                    TopicName = teacherTopic,
                    ExpPoints = s?.ExpPoints,
                    BirthDate = s?.BirthDate,
                    DateJoined = s?.DateJoined ?? t?.DateJoined ?? a?.DateJoined ?? default,
                    Status = l.Status,
                    Password = l.Password,
                };
            })
            .OrderBy(u => u.Role)
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

            var teacher = await _db.Teachers.FindAsync(userId);
            if (teacher != null) _db.Teachers.Remove(teacher);

            var admin = await _db.Admins.FindAsync(userId);
            if (admin != null) _db.Admins.Remove(admin);

            _db.LoginCredentials.Remove(login);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{userId}' deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(
            string userId, string name, string email, string phoneNumber,
            int? age, string? level, string? school, string? newPassword)
        {
            if (!string.IsNullOrEmpty(newPassword))
            {
                var login = await _db.LoginCredentials.FindAsync(userId);
                if (login != null)
                {
                    login.Password = newPassword; // TODO: hash before storing
                }
            }
            
            var student = await _db.Students.FindAsync(userId);
            if (student != null)
            {
                student.StudentName = name;
                student.Email = email;
                student.PhoneNumber = phoneNumber;
                if (age.HasValue) student.StudentAge = age.Value;
                student.StudyLevel = level ?? student.StudyLevel;
                student.School = school ?? student.School;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' updated.";
                return RedirectToPage();
            }

            var teacher = await _db.Teachers.FindAsync(userId);
            if (teacher != null)
            {
                teacher.TeacherName = name;
                teacher.Email = email;
                teacher.PhoneNumber = phoneNumber;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' updated.";
                return RedirectToPage();
            }

            var admin = await _db.Admins.FindAsync(userId);
            if (admin != null)
            {
                admin.AdminName = name;
                admin.Email = email;
                admin.PhoneNumber = phoneNumber;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{userId}' updated.";
            }

            return RedirectToPage();
        }
    }
}