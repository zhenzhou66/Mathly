using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Admin
{
    public class TeacherManagementModel : PageModel
    {
        private readonly MathlyDbContext _db;
        public TeacherManagementModel(MathlyDbContext db) => _db = db;

        public List<TeacherRow> Teachers { get; set; } = new();
        public List<TopicOption> AllTopics { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class TeacherRow
        {
            public string UserID { get; set; } = "";
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public string Qualification { get; set; } = "";
            public string TopicsDisplay { get; set; } = "";
            public string AssignedTopicIds { get; set; } = ""; 
            public int StudentCount { get; set; }
            public string Status { get; set; } = "";
        }

        public class TopicOption
        {
            public string TopicID { get; set; } = "";
            public string TopicName { get; set; } = "";
        }

        [BindProperty] public string NewTitle { get; set; } = "Mr.";
        [BindProperty] public string NewName { get; set; } = "";
        [BindProperty] public string NewEmail { get; set; } = "";
        [BindProperty] public string NewPhone { get; set; } = "";
        [BindProperty] public string NewQualification { get; set; } = "";
        [BindProperty] public string NewPassword { get; set; } = "";
        [BindProperty] public List<string>? NewTopicIds { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var teachers = await _db.Teachers.ToListAsync();
            var logins = await _db.LoginCredentials.ToDictionaryAsync(l => l.UserID);
            var topics = await _db.Topics.ToListAsync();
            var studentTopics = await _db.StudentTopics.ToListAsync();

            AllTopics = topics
                .Select(t => new TopicOption { TopicID = t.TopicID, TopicName = t.TopicName })
                .OrderBy(t => t.TopicName)
                .ToList();

            Teachers = teachers.Select(t =>
            {
                var myTopics = topics.Where(top => top.TeacherID == t.UserID).ToList();
                var topicIds = myTopics.Select(x => x.TopicID).ToList();
                var studentCount = studentTopics
                    .Where(st => topicIds.Contains(st.TopicID))
                    .Select(st => st.StudentID)
                    .Distinct()
                    .Count();

                return new TeacherRow
                {
                    UserID = t.UserID,
                    Name = t.TeacherName ?? "",
                    Email = t.Email ?? "No Email",
                    PhoneNumber = t.PhoneNumber ?? "-",
                    Qualification = t.HighestQualification ?? "-",
                    TopicsDisplay = myTopics.Any() ? string.Join(", ", myTopics.Select(x => x.TopicName)) : "—",
                    AssignedTopicIds = string.Join(",", topicIds),
                    StudentCount = studentCount,
                    Status = logins.TryGetValue(t.UserID, out var l) ? l.Status : "active"
                };
            })
            .OrderBy(t => t.Name)
            .ToList();
        }

        public async Task<IActionResult> OnPostAddTeacherAsync()
        {
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewEmail) || string.IsNullOrWhiteSpace(NewPassword))
            {
                TempData["Error"] = "Name, email, and initial password are required.";
                return RedirectToPage();
            }

            string formattedName = $"{NewTitle} {NewName.Trim()}";

            var count = await _db.Teachers.CountAsync();
            var newId = $"teacher{(count + 1):D3}";
            while (await _db.LoginCredentials.FindAsync(newId) != null)
                newId += "x";

            _db.LoginCredentials.Add(new LoginCredentials
            {
                UserID = newId,
                Password = NewPassword, // TODO: hash before storing
                Role = "teacher",
                Status = "active"
            });

            _db.Teachers.Add(new TeacherInfo
            {
                UserID = newId,
                TeacherName = formattedName,
                Email = NewEmail,
                PhoneNumber = NewPhone,
                HighestQualification = NewQualification,
                DateJoined = DateOnly.FromDateTime(DateTime.Today)
            });

            await _db.SaveChangesAsync();

            if (NewTopicIds != null && NewTopicIds.Any())
            {
                var topicsToAssign = await _db.Topics.Where(t => NewTopicIds.Contains(t.TopicID)).ToListAsync();
                foreach (var top in topicsToAssign) top.TeacherID = newId;
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = $"Teacher '{NewName}' added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignTopicsAsync(string teacherId, List<string>? topicIds)
        {
            topicIds ??= new List<string>();

            // Unassign topics this teacher currently holds but that were unchecked
            var currentlyAssigned = await _db.Topics.Where(t => t.TeacherID == teacherId).ToListAsync();
            foreach (var t in currentlyAssigned.Where(t => !topicIds.Contains(t.TopicID)))
                t.TeacherID = null;

            // Assign newly checked topics (this also reassigns a topic away from its previous teacher, if any)
            var toAssign = await _db.Topics.Where(t => topicIds.Contains(t.TopicID)).ToListAsync();
            foreach (var t in toAssign)
                t.TeacherID = teacherId;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Topics updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSuspendAsync(string teacherId)
        {
            var login = await _db.LoginCredentials.FindAsync(teacherId);
            if (login != null)
            {
                login.Status = "suspended";
                await _db.SaveChangesAsync();
                TempData["Success"] = "Teacher suspended.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(string teacherId)
        {
            var login = await _db.LoginCredentials.FindAsync(teacherId);
            if (login != null)
            {
                login.Status = "active";
                await _db.SaveChangesAsync();
                TempData["Success"] = "Teacher restored.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTeacherAsync(string teacherId)
        {
            var login = await _db.LoginCredentials.FindAsync(teacherId);
            if (login == null || login.Status != "suspended")
            {
                TempData["Error"] = "Only suspended teachers can be deleted.";
                return RedirectToPage();
            }

            var topics = await _db.Topics.Where(t => t.TeacherID == teacherId).ToListAsync();
            foreach (var t in topics) t.TeacherID = null;

            var teacher = await _db.Teachers.FindAsync(teacherId);
            if (teacher != null) _db.Teachers.Remove(teacher);

            _db.LoginCredentials.Remove(login);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Teacher deleted.";
            return RedirectToPage();
        }
    }
}