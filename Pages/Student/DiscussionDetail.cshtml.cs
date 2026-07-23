using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Utils;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class DiscussionDetailModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DiscussionDetailModel(MathlyDbContext db) => _db = db;

        [BindProperty]
        public string NewComment { get; set; }

        public string DiscussionID { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionText { get; set; }
        public string AuthorName { get; set; }
        public DateTime PostedDate { get; set; }
        public string PostedAgo => TimeAgo.Format(PostedDate);
        public string ErrorMessage { get; set; }
        public List<CommentRow> Comments { get; set; } = new();

        public class CommentRow
        {
            public string CommentID { get; set; }
            public string CommentText { get; set; }
            public string AuthorName { get; set; }
            public int LikeCount { get; set; }
            public DateTime PostedDate { get; set; }
            public string PostedAgo => TimeAgo.Format(PostedDate);
        }

        private class DiscussionDto
        {
            public string DiscussionID { get; set; }
            public string QuestionTitle { get; set; }
            public string QuestionText { get; set; }
            public string AuthorName { get; set; }
            public DateTime PostedDate { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string discussionId)
        {
            if (!await LoadDiscussionAsync(discussionId))
                return RedirectToPage("/Student/Discussion");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string discussionId)
        {
            if (!await LoadDiscussionAsync(discussionId))
                return RedirectToPage("/Student/Discussion");

            if (string.IsNullOrWhiteSpace(NewComment))
            {
                ErrorMessage = "Please enter a comment before submitting.";
                return Page();
            }

            var commentNum = await GetNextIdNumberAsync("comment", "commentID", "cmt");

            // comment.likeCount is NOT NULL with no default, so this goes through raw
            // SQL to supply it explicitly rather than relying on EF's change tracker.
            await _db.Database.ExecuteSqlRawAsync(
                "INSERT INTO comment (commentID, userID, discussionID, commentText, likeCount) VALUES ({0}, {1}, {2}, {3}, {4})",
                $"cmt{commentNum:D9}", StudentID, discussionId, NewComment.Trim(), 0);

            return RedirectToPage(new { discussionId });
        }

        private async Task<bool> LoadDiscussionAsync(string discussionId)
        {
            if (string.IsNullOrWhiteSpace(discussionId))
                return false;

            var discussion = await _db.Database
                .SqlQueryRaw<DiscussionDto>(
                    @"SELECT d.discussionID AS DiscussionID, d.questionTitle AS QuestionTitle, d.questionText AS QuestionText,
                             COALESCE(si.studentName, ti.teacherName, d.userID) AS AuthorName,
                             d.postedDate AS PostedDate
                      FROM discussion d
                      LEFT JOIN studentinfo si ON si.userID = d.userID
                      LEFT JOIN teacherinfo ti ON ti.userID = d.userID
                      WHERE d.discussionID = {0}", discussionId)
                .FirstOrDefaultAsync();

            if (discussion == null)
                return false;

            DiscussionID = discussion.DiscussionID;
            QuestionTitle = discussion.QuestionTitle;
            QuestionText = discussion.QuestionText;
            AuthorName = discussion.AuthorName;
            PostedDate = discussion.PostedDate;

            Comments = await _db.Database
                .SqlQueryRaw<CommentRow>(
                    @"SELECT c.commentID AS CommentID, c.commentText AS CommentText,
                             COALESCE(si.studentName, ti.teacherName, c.userID) AS AuthorName,
                             c.likeCount AS LikeCount,
                             c.postedDate AS PostedDate
                      FROM comment c
                      LEFT JOIN studentinfo si ON si.userID = c.userID
                      LEFT JOIN teacherinfo ti ON ti.userID = c.userID
                      WHERE c.discussionID = {0}
                      ORDER BY c.postedDate ASC", discussionId)
                .ToListAsync();

            return true;
        }

        // Toggles a comment's like count up/down. Note: the schema has no
        // per-user "who liked what" table, just a raw counter on the comment
        // itself, so this can't stop the same student from liking a comment
        // more than once across page reloads — it only tracks the running total.
        public async Task<IActionResult> OnPostToggleLikeAsync(string commentId, bool increment)
        {
            if (!string.IsNullOrWhiteSpace(commentId))
            {
                var delta = increment ? 1 : -1;
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE comment SET likeCount = GREATEST(likeCount + {0}, 0) WHERE commentID = {1}",
                    delta, commentId);
            }

            return new EmptyResult();
        }

        // Same numbering scheme as Discussion.cshtml.cs / AttemptQuiz.cshtml.cs:
        // find the highest existing "<prefix><digits>" ID and return the next number.
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
    }
}
