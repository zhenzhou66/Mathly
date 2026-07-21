using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class DiscussionModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DiscussionModel(MathlyDbContext db) => _db = db;

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
        }

        public async Task OnGetAsync()
        {
            await LoadThreadsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewQuestion))
            {
                ErrorMessage = "Please fill in both a title and your question.";
                await LoadThreadsAsync();
                return Page();
            }

            _db.Discussions.Add(new Discussion
            {
                DiscussionID = Guid.NewGuid().ToString("N"),
                UserID = StudentID,
                QuestionTitle = NewTitle.Trim(),
                QuestionText = NewQuestion.Trim()
            });
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task LoadThreadsAsync()
        {
            Threads = (await _db.Database
                .SqlQueryRaw<DiscussionRow>(
                    @"SELECT d.discussionID AS DiscussionID, d.questionTitle AS QuestionTitle, d.questionText AS QuestionText,
                             COALESCE(si.studentName, ti.teacherName, d.userID) AS AuthorName,
                             (SELECT COUNT(*) FROM comment c WHERE c.discussionID = d.discussionID) AS CommentCount
                      FROM discussion d
                      LEFT JOIN studentinfo si ON si.userID = d.userID
                      LEFT JOIN teacherinfo ti ON ti.userID = d.userID
                      ORDER BY d.discussionID DESC")
                .ToListAsync());
        }
    }
}
