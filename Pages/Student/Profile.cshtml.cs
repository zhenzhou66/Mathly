using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    public class ProfileModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private const string StudentID = "student001"; // TODO: replace with session (#4)

        public ProfileModel(MathlyDbContext db) => _db = db;

        public string StudentName { get; set; } = "Student";
        public string StudyLevel { get; set; } = "";
        public string School { get; set; } = "";
        public int ExpPoints { get; set; }
        public string MemberSince { get; set; } = "";
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string PasswordSuccessMessage { get; set; }
        public string PasswordErrorMessage { get; set; }

        [BindProperty]
        public string StudentNameInput { get; set; } = "";

        [BindProperty]
        public int AgeInput { get; set; }

        [BindProperty]
        public string EmailInput { get; set; } = "";

        [BindProperty]
        public string PhoneNumberInput { get; set; } = "";

        [BindProperty]
        public string SchoolInput { get; set; } = "";

        [BindProperty]
        public string StudyLevelInput { get; set; } = "";

        [BindProperty]
        public DateOnly BirthDateInput { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; } = "";

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public string ConfirmPassword { get; set; } = "";

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["ProfileSuccess"] as string;
            ErrorMessage = TempData["ProfileError"] as string;
            PasswordSuccessMessage = TempData["PasswordSuccess"] as string;
            PasswordErrorMessage = TempData["PasswordError"] as string;

            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                StudentName = student.StudentName;
                StudyLevel = student.StudyLevel;
                School = student.School;
                ExpPoints = student.ExpPoints;
                MemberSince = student.DateJoined.ToString("MMMM yyyy");

                StudentNameInput = student.StudentName;
                AgeInput = student.StudentAge;
                EmailInput = student.Email;
                PhoneNumberInput = student.PhoneNumber;
                SchoolInput = student.School;
                StudyLevelInput = student.StudyLevel;
                BirthDateInput = student.BirthDate;
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                student.StudentName = StudentNameInput;
                student.StudentAge = AgeInput;
                student.Email = EmailInput;
                student.PhoneNumber = PhoneNumberInput;
                student.School = SchoolInput;
                student.StudyLevel = StudyLevelInput;
                student.BirthDate = BirthDateInput;
                await _db.SaveChangesAsync();
                TempData["ProfileSuccess"] = "Profile updated successfully!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var credentials = await _db.LoginCredentials.FindAsync(StudentID);
            if (credentials == null || credentials.Password != CurrentPassword)
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword != ConfirmPassword)
            {
                TempData["PasswordError"] = "New password and confirmation do not match.";
                return RedirectToPage();
            }

            credentials.Password = NewPassword;
            await _db.SaveChangesAsync();
            TempData["PasswordSuccess"] = "Password changed successfully!";
            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            return RedirectToPage("/Login");
        }
    }
}
