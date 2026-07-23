using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;
using Mathly.Utils;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class DiscussionModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DiscussionModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";

        [BindProperty]
        public string NewTitle { get; set; }

        [BindProperty]
        public string NewQuestion { get; set; }

        public string ErrorMessage { get; set; }
        public List<DiscussionRow> Threads { get; set; } = new();

        public class DiscussionRow
        {
            public string DiscussionID { get; set; }
            public string QuestionTitle { get; set; }
            public string QuestionText { get; set; }
            public string AuthorName { get; set; }
            public int CommentCount { get; set; }
            public DateTime PostedDate { get; set; }
            public string PostedAgo => TimeAgo.Format(PostedDate);
        }

        public async Task OnGetAsync()
        {
            await LoadPageDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewQuestion))
            {
                ErrorMessage = "Please fill in both a title and your question.";
                await LoadPageDataAsync();
                return Page();
            }

            var discussionNum = await GetNextIdNumberAsync("discussion", "discussionID", "disc");

            _db.Discussions.Add(new Discussion
            {
                DiscussionID = $"disc{discussionNum:D9}",
                UserID = TeacherID,
                QuestionTitle = NewTitle.Trim(),
                QuestionText = NewQuestion.Trim()
            });
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task<int> GetNextIdNumberAsync(string table, string column, string prefix)
        {
            var maxNumber = await _db.Database
                .SqlQueryRaw<int>(
                    $@"SELECT COALESCE(MAX(CAST(SUBSTRING({column}, {prefix.Length + 1}) AS UNSIGNED)), 0) AS Value
                       FROM {table}
                       WHERE {column} REGEXP '^{prefix}[0-9]+$'")
                .FirstOrDefaultAsync();

            return maxNumber + 1;
        }

        private async Task LoadPageDataAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
            {
                TeacherName = teacher.TeacherName;
            }

            Threads = (await _db.Database
                .SqlQueryRaw<DiscussionRow>(
                    @"SELECT d.discussionID AS DiscussionID, d.questionTitle AS QuestionTitle, d.questionText AS QuestionText,
                             COALESCE(si.studentName, ti.teacherName, d.userID) AS AuthorName,
                             (SELECT COUNT(*) FROM comment c WHERE c.discussionID = d.discussionID) AS CommentCount,
                             d.postedDate AS PostedDate
                      FROM discussion d
                      LEFT JOIN studentinfo si ON si.userID = d.userID
                      LEFT JOIN teacherinfo ti ON ti.userID = d.userID
                      ORDER BY d.postedDate DESC")
                .ToListAsync());
        }
    }
}
