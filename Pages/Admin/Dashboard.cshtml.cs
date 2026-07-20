using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly MathlyDbContext _db;
        public DashboardModel(MathlyDbContext db) => _db = db;

        private const string AdminID = "admin001"; // TODO: replace with session (#4)

        public string AdminName { get; set; } = "Admin";
        public int TotalStudents { get; set; }
        public int NewStudentsThisMonth { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveTeachers { get; set; }
        public int TotalTopics { get; set; }
        public int NewTopicsThisMonth { get; set; }
        public int TotalXPAwarded { get; set; }
        public int XPIncreaseToday { get; set; }
        public List<TopicPerformance> TopicPerformanceList { get; set; } = new();

        public class TopicPerformance
        {
            public string TopicID { get; set; } = string.Empty;
            public string TopicName { get; set; } = string.Empty;
            public double PerformancePercentage { get; set; }
            public int TotalStudents { get; set; }
        }

        private class CountDto
        {
            public int Value { get; set; }
        }

        private class ScoreDto
        {
            public double Score { get; set; }
        }

        public async Task OnGetAsync()
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.UserID == AdminID);
            if (admin != null)
            {
                AdminName = admin.AdminName;
            }

            TotalStudents = await _db.Students.CountAsync();
            TotalTeachers = await _db.Teachers.CountAsync();
            TotalTopics = await _db.Topics.CountAsync();

            var allStudents = await _db.Students.ToListAsync();
            TotalXPAwarded = allStudents.Sum(s => s.ExpPoints);

            var today = DateTime.Now;
            NewStudentsThisMonth = allStudents.Count(s => s.DateJoined.Month == today.Month && s.DateJoined.Year == today.Year);

            ActiveTeachers = TotalTeachers; // TODO: no isActive column yet
            NewTopicsThisMonth = 0; // TODO: no dateCreated column on topic yet
            XPIncreaseToday = 0; // TODO: no XP history table yet

            var allTopics = await _db.Topics.ToListAsync();

            foreach (var topic in allTopics)
            {
                var studentCount = await _db.Database.SqlQueryRaw<CountDto>(
                    "SELECT COUNT(DISTINCT userID) AS Value FROM StudentTopic WHERE topicID = {0}", topic.TopicID
                ).FirstOrDefaultAsync();

                var topicScores = await _db.Database.SqlQueryRaw<ScoreDto>(
                    "SELECT qr.score AS Score FROM QuizResult qr JOIN Quizzes q ON qr.quizID = q.quizID WHERE q.topicID = {0}", topic.TopicID
                ).ToListAsync();

                double avgPercentage = topicScores.Any() ? topicScores.Average(s => s.Score) : 0;

                TopicPerformanceList.Add(new TopicPerformance
                {
                    TopicID = topic.TopicID,
                    TopicName = topic.TopicName,
                    PerformancePercentage = avgPercentage,
                    TotalStudents = studentCount?.Value ?? 0
                });
            }
        }
    }
}