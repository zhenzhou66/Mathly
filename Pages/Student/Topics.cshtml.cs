using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class TopicsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public TopicsModel(MathlyDbContext db) => _db = db;

        public List<MyTopicRow> MyTopics { get; set; } = new();
        public List<AvailableTopicRow> AvailableTopics { get; set; } = new();
        public string? StatusMessage { get; set; }

        public class MyTopicRow
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
            public string? TeacherName { get; set; }
            public double ProgressPercentage { get; set; }
        }

        public class AvailableTopicRow
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
            public string? TeacherName { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        // Joining a topic writes to `studenttopic`, whose student-id column is
        // physically named `userID` (not `studentID` like the C# model property),
        // so this goes through raw SQL rather than EF's change tracker.
        public async Task<IActionResult> OnPostJoinAsync(string topicId)
        {
            if (!string.IsNullOrWhiteSpace(topicId))
            {
                var alreadyJoined = await _db.Database
                    .SqlQueryRaw<int>(
                        "SELECT COUNT(*) AS Value FROM studenttopic WHERE userID = {0} AND topicID = {1}",
                        StudentID, topicId)
                    .FirstOrDefaultAsync();

                if (alreadyJoined == 0)
                {
                    var newId = Guid.NewGuid().ToString("N");
                    await _db.Database.ExecuteSqlRawAsync(
                        "INSERT INTO studenttopic (studentTopicID, userID, topicID, selectedDate) VALUES ({0}, {1}, {2}, {3})",
                        newId, StudentID, topicId, DateOnly.FromDateTime(DateTime.UtcNow));
                    StatusMessage = "Topic added!";
                }
            }

            await LoadAsync();
            return Page();
        }

        private async Task LoadAsync()
        {
            // Progress = quizzes the student has attempted / total quizzes for the topic
            // (not the `learningprogress` table, which isn't kept in sync with attempts).
            MyTopics = (await _db.Database
                .SqlQueryRaw<MyTopicRow>(
                    @"SELECT t.topicID AS TopicID, t.topicName AS TopicName, ti.teacherName AS TeacherName,
                             CASE WHEN tot.TotalQuizzes > 0
                                  THEN ROUND(COALESCE(comp.CompletedQuizzes, 0) * 100.0 / tot.TotalQuizzes, 1)
                                  ELSE 0 END AS ProgressPercentage
                      FROM studenttopic st
                      JOIN topic t ON st.topicID = t.topicID
                      LEFT JOIN teacherinfo ti ON t.userID = ti.userID
                      LEFT JOIN (
                          SELECT topicID, COUNT(*) AS TotalQuizzes
                          FROM quizzes
                          GROUP BY topicID
                      ) tot ON tot.topicID = t.topicID
                      LEFT JOIN (
                          SELECT q.topicID, COUNT(DISTINCT q.quizID) AS CompletedQuizzes
                          FROM quizzes q
                          JOIN quizresult qr ON qr.quizID = q.quizID AND qr.userID = {0}
                          GROUP BY q.topicID
                      ) comp ON comp.topicID = t.topicID
                      WHERE st.userID = {0}
                      ORDER BY t.topicName", StudentID)
                .ToListAsync());

            AvailableTopics = (await _db.Database
                .SqlQueryRaw<AvailableTopicRow>(
                    @"SELECT t.topicID AS TopicID, t.topicName AS TopicName, ti.teacherName AS TeacherName
                      FROM topic t
                      LEFT JOIN teacherinfo ti ON t.userID = ti.userID
                      WHERE t.topicID NOT IN (SELECT topicID FROM studenttopic WHERE userID = {0})
                      ORDER BY t.topicName", StudentID)
                .ToListAsync());
        }
    }
}
