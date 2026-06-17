using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    public class DashboardModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private const string TeacherID = "teacher001"; // TODO: replace with session

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

        public async Task OnGetAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
                TeacherName = teacher.TeacherName;

            var topicIds = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .Select(t => t.TopicID)
                .ToListAsync();

            var quizIds = await _db.Quizzes
                .Where(q => topicIds.Contains(q.TopicID))
                .Select(q => q.QuizID)
                .ToListAsync();

            TotalQuizzes = quizIds.Count;

            TotalStudents = await _db.StudentTopics
                .Where(st => topicIds.Contains(st.TopicID))
                .Select(st => st.StudentID)
                .Distinct()
                .CountAsync();

            var allResults = await _db.QuizResults
                .Where(r => quizIds.Contains(r.QuizID))
                .ToListAsync();

            if (allResults.Any())
                AvgClassScore = $"{allResults.Average(r => r.Score):0}%";

            // Discussions with no replies are unanswered
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

            // Build topic rows
            var topics = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .ToListAsync();

            foreach (var t in topics)
            {
                var tQuizIds = await _db.Quizzes
                    .Where(q => q.TopicID == t.TopicID)
                    .Select(q => q.QuizID)
                    .ToListAsync();

                var tStudents = await _db.StudentTopics
                    .Where(st => st.TopicID == t.TopicID)
                    .Select(st => st.StudentID)
                    .Distinct()
                    .CountAsync();

                var tResults = await _db.QuizResults
                    .Where(r => tQuizIds.Contains(r.QuizID))
                    .ToListAsync();

                Topics.Add(new TopicRow
                {
                    TopicID = t.TopicID,
                    TopicName = t.TopicName,
                    QuizCount = tQuizIds.Count,
                    StudentCount = tStudents,
                    AvgScore = tResults.Any() ? $"{tResults.Average(r => r.Score):0}%" : "—"
                });
            }

            // Recent quiz results
            var rawResults = await _db.QuizResults
                .Where(r => quizIds.Contains(r.QuizID))
                .Include(r => r.Student)
                .Include(r => r.Quiz)
                .OrderByDescending(r => r.ResultID)
                .Take(5)
                .ToListAsync();

            RecentResults = rawResults.Select(r => new RecentResult
            {
                StudentName = r.Student?.StudentName ?? "Student",
                QuizTitle = r.Quiz?.QuizTitle ?? "Quiz",
                Score = $"{r.Score:0}%"
            }).ToList();
        }
    }
}
