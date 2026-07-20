using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class ProfileModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public ProfileModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";
        public string DateJoined { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        [BindProperty]
        public string TeacherNameInput { get; set; } = "";

        [BindProperty]
        public string PhoneNumber { get; set; } = "";

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string HighestQualification { get; set; } = "";

        public async Task OnGetAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
            {
                TeacherName = teacher.TeacherName;
                TeacherNameInput = teacher.TeacherName;
                PhoneNumber = teacher.PhoneNumber;
                Email = teacher.Email;
                HighestQualification = teacher.HighestQualification;
                DateJoined = teacher.DateJoined.ToString("MMMM yyyy");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
            {
                teacher.TeacherName = TeacherNameInput;
                teacher.PhoneNumber = PhoneNumber;
                teacher.Email = Email;
                teacher.HighestQualification = HighestQualification;
                await _db.SaveChangesAsync();
                TeacherName = teacher.TeacherName;
                DateJoined = teacher.DateJoined.ToString("MMMM yyyy");
                SuccessMessage = "Profile updated successfully!";
            }
            return Page();
        }
    }
}
