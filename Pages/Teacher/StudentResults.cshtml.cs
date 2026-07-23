using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class StudentResultsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public StudentResultsModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";
        public string ActiveTopicId { get; set; } = "";
        public List<TopicTab> Topics { get; set; } = new();
        public List<ResultRow> Results { get; set; } = new();

        public int TotalAttempts { get; set; }
        public int UniqueStudents { get; set; }
        public string AvgScore { get; set; } = "—";

        public class TopicTab
        {
            public string TopicId { get; set; } = "";
            public string TopicName { get; set; } = "";
        }

        public class ResultRow
        {
            public string StudentName { get; set; } = "";
            public string QuizTitle { get; set; } = "";
            public string TopicName { get; set; } = "";
            public double Score { get; set; }
            public int Correct { get; set; }
            public int Total { get; set; }
        }

        public async Task OnGetAsync(string? topicId)
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
                TeacherName = teacher.TeacherName;

            var teacherTopicIds = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .Select(t => t.TopicID)
                .ToListAsync();

            Topics = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .OrderBy(t => t.TopicName)
                .Select(t => new TopicTab { TopicId = t.TopicID, TopicName = t.TopicName })
                .ToListAsync();

            var quizQuery = _db.Quizzes.Where(q => teacherTopicIds.Contains(q.TopicID));

            if (!string.IsNullOrWhiteSpace(topicId) && teacherTopicIds.Contains(topicId))
            {
                ActiveTopicId = topicId;
                quizQuery = quizQuery.Where(q => q.TopicID == topicId);
            }

            var quizIds = await quizQuery.Select(q => q.QuizID).ToListAsync();

            Results = await (
                from r in _db.QuizResults
                join q in _db.Quizzes on r.QuizID equals q.QuizID
                join t in _db.Topics on q.TopicID equals t.TopicID
                join s in _db.Students on r.StudentID equals s.UserID
                where quizIds.Contains(r.QuizID)
                orderby r.Score descending
                select new ResultRow
                {
                    StudentName = s.StudentName,
                    QuizTitle = q.QuizTitle,
                    TopicName = t.TopicName,
                    Score = r.Score,
                    Correct = r.TotalCorrectAnswer,
                    Total = r.TotalQuestionsAmount
                }
            ).ToListAsync();

            TotalAttempts = Results.Count;
            UniqueStudents = Results.Select(r => r.StudentName).Distinct().Count();
            AvgScore = Results.Any()
                ? Results.Average(r => r.Score).ToString("F1") + "%"
                : "—";
        }
    }
}
