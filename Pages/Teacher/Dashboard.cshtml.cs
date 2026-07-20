using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class DashboardModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DashboardModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";
        public int TotalStudents { get; set; }
        public int TotalQuizzes { get; set; }
        public string AvgClassScore { get; set; } = "—";
        public int UnansweredCount { get; set; }
        public List<TopicRow> Topics { get; set; } = new();
        public List<Discussion> PendingDiscussions { get; set; } = new();
        public List<RecentResult> RecentResults { get; set; } = new();

        public class TopicRow
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
            public int QuizCount { get; set; }
            public int StudentCount { get; set; }
            public string AvgScore { get; set; }
        }

        public class RecentResult
        {
            public string StudentName { get; set; }
            public string QuizTitle { get; set; }
            public string Score { get; set; }
        }

        private class TopicDto
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
        }

        private class CountDto
        {
            public int Value { get; set; }
        }

        private class ScoreDto
        {
            public double Score { get; set; }
            public string QuizID { get; set; }
        }

        private class RecentDto
        {
            public string StudentName { get; set; }
            public string QuizTitle { get; set; }
            public double Score { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Get teacher name
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
                TeacherName = teacher.TeacherName;

            // Raw SQL: Get topics for this teacher (topic table uses userID column)
            var topicDtos = await _db.Database
                .SqlQueryRaw<TopicDto>(
                    "SELECT topicID AS TopicID, topicName AS TopicName FROM topic WHERE userID = {0}", TeacherID)
                .ToListAsync();

            var topicIds = topicDtos.Select(t => t.TopicID).ToList();
            if (!topicIds.Any()) return;

            var topicIdList = string.Join(",", topicIds.Select(id => $"'{id}'"));

            // Get quizzes for these topics
            var allQuizzes = await _db.Quizzes
                .Where(q => topicIds.Contains(q.TopicID))
                .ToListAsync();

            var quizIds = allQuizzes.Select(q => q.QuizID).ToList();
            TotalQuizzes = quizIds.Count;

            // Raw SQL: Count distinct students in these topics (studenttopic uses userID column)
            var studentCountDto = await _db.Database
                .SqlQueryRaw<CountDto>(
                    $"SELECT COUNT(DISTINCT userID) AS Value FROM studenttopic WHERE topicID IN ({topicIdList})")
                .FirstOrDefaultAsync();
            TotalStudents = studentCountDto?.Value ?? 0;

            // Get quiz results and calculate average score
            List<ScoreDto> allScores = new();
            if (quizIds.Any())
            {
                var quizIdList = string.Join(",", quizIds.Select(id => $"'{id}'"));

                // Raw SQL: Get all scores (quizresult uses userID column)
                allScores = await _db.Database
                    .SqlQueryRaw<ScoreDto>(
                        $"SELECT score AS Score, quizID AS QuizID FROM quizresult WHERE quizID IN ({quizIdList})")
                    .ToListAsync();

                if (allScores.Any())
                    AvgClassScore = $"{allScores.Average(r => r.Score):0}%";

                // Raw SQL: Get recent quiz activity
                RecentResults = (await _db.Database
                    .SqlQueryRaw<RecentDto>(
                        $@"SELECT si.studentName AS StudentName, q.quizTitle AS QuizTitle, qr.score AS Score
                           FROM quizresult qr
                           JOIN studentinfo si ON qr.userID = si.userID
                           JOIN quizzes q ON qr.quizID = q.quizID
                           WHERE qr.quizID IN ({quizIdList})
                           ORDER BY qr.resultID DESC
                           LIMIT 5")
                    .ToListAsync())
                    .Select(r => new RecentResult
                    {
                        StudentName = r.StudentName,
                        QuizTitle = r.QuizTitle,
                        Score = $"{r.Score:0}%"
                    }).ToList();
            }

            // Get unanswered discussions using LINQ (comment/discussion UserID matches schema)
            var answeredIds = await _db.Comments
                .Select(c => c.DiscussionID)
                .Distinct()
                .ToListAsync();

            PendingDiscussions = await _db.Discussions
                .Where(d => !answeredIds.Contains(d.DiscussionID))
                .Take(5)
                .ToListAsync();

            UnansweredCount = await _db.Discussions
                .CountAsync(d => !answeredIds.Contains(d.DiscussionID));

            // Build topics table
            foreach (var t in topicDtos)
            {
                var tQuizIds = allQuizzes
                    .Where(q => q.TopicID == t.TopicID)
                    .Select(q => q.QuizID)
                    .ToList();

                // Raw SQL: Count students for this topic
                var tStudentCountDto = await _db.Database
                    .SqlQueryRaw<CountDto>(
                        "SELECT COUNT(DISTINCT userID) AS Value FROM studenttopic WHERE topicID = {0}", t.TopicID)
                    .FirstOrDefaultAsync();

                var tScores = allScores.Where(r => tQuizIds.Contains(r.QuizID)).ToList();

                Topics.Add(new TopicRow
                {
                    TopicID = t.TopicID,
                    TopicName = t.TopicName,
                    QuizCount = tQuizIds.Count,
                    StudentCount = tStudentCountDto?.Value ?? 0,
                    AvgScore = tScores.Any() ? $"{tScores.Average(r => r.Score):0}%" : "—"
                });
            }
        }
    }
}
