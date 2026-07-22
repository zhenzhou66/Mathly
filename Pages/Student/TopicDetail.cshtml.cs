using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class TopicDetailModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public TopicDetailModel(MathlyDbContext db) => _db = db;

        public string TopicID { get; set; }
        public string TopicName { get; set; }
        public string TeacherName { get; set; }
        public double ProgressPercentage { get; set; }

        public List<QuizRow> Quizzes { get; set; } = new();
        public List<MaterialRow> Materials { get; set; } = new();

        public class QuizRow
        {
            public string QuizID { get; set; }
            public string QuizTitle { get; set; }
            public double? BestScore { get; set; }
        }

        public class MaterialRow
        {
            public string MaterialID { get; set; }
            public string FileName { get; set; }
        }

        private class TopicDto
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
            public string TeacherName { get; set; }
            public double ProgressPercentage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string topicId)
        {
            if (string.IsNullOrWhiteSpace(topicId))
                return RedirectToPage("/Student/Topics");

            // Only students who've joined a topic can see its quizzes/materials.
            // studenttopic's student-id column is `userID`, not `StudentID`, so
            // this is raw SQL rather than a plain EF query (same reason as
            // everywhere else this table gets touched).
            var isJoined = await _db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM studenttopic WHERE userID = {0} AND topicID = {1}",
                    StudentID, topicId)
                .FirstOrDefaultAsync();

            if (isJoined == 0)
                return RedirectToPage("/Student/Topics");

            var topic = await _db.Database
                .SqlQueryRaw<TopicDto>(
                    @"SELECT t.topicID AS TopicID, t.topicName AS TopicName, ti.teacherName AS TeacherName,
                             COALESCE(lp.progressPercentage, 0) AS ProgressPercentage
                      FROM topic t
                      LEFT JOIN teacherinfo ti ON t.userID = ti.userID
                      LEFT JOIN learningprogress lp ON lp.topicID = t.topicID AND lp.userID = {0}
                      WHERE t.topicID = {1}", StudentID, topicId)
                .FirstOrDefaultAsync();

            if (topic == null)
                return RedirectToPage("/Student/Topics");

            TopicID = topic.TopicID;
            TopicName = topic.TopicName;
            TeacherName = topic.TeacherName;
            ProgressPercentage = topic.ProgressPercentage;

            Quizzes = (await _db.Database
                .SqlQueryRaw<QuizRow>(
                    @"SELECT q.quizID AS QuizID, q.quizTitle AS QuizTitle, best.BestScore AS BestScore
                      FROM quizzes q
                      LEFT JOIN (
                          SELECT quizID, userID, MAX(score) AS BestScore
                          FROM quizresult
                          GROUP BY quizID, userID
                      ) best ON best.quizID = q.quizID AND best.userID = {0}
                      WHERE q.topicID = {1}
                      ORDER BY q.quizTitle", StudentID, topicId)
                .ToListAsync());

            // studymaterial.fileName is stored as a blob, so CAST(... AS CHAR)
            // mirrors the pattern already used in Materials.cshtml.cs.
            Materials = (await _db.Database
                .SqlQueryRaw<MaterialRow>(
                    @"SELECT materialID AS MaterialID, CAST(fileName AS CHAR) AS FileName
                      FROM studymaterial
                      WHERE topicID = {0}
                      ORDER BY fileName", topicId)
                .ToListAsync());

            return Page();
        }
    }
}
