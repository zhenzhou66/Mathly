using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class ProgressModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public ProgressModel(MathlyDbContext db)
        {
            _db = db;
        }

        public string StudentName { get; set; } = "Student";
        public int ExpPoints { get; set; }
        public int DayStreak { get; set; } = 3;
        public int ClassRank { get; set; } = 1;
        public int QuizzesCompleted { get; set; }

        public List<TopicMasteryDto> TopicMasteryList { get; set; } = new();
        public List<QuizHistoryDto> QuizHistoryList { get; set; } = new();
        public List<DayXpDto> WeeklyChartData { get; set; } = new();

        public class TopicMasteryDto
        {
            public string TopicName { get; set; }
            public double MasteryPercentage { get; set; }
        }

        public class QuizHistoryDto
        {
            public string QuizTitle { get; set; }
            public string TopicName { get; set; }
            public double Score { get; set; }
            public int CorrectCount { get; set; }
            public int TotalCount { get; set; }
        }

        public class DayXpDto
        {
            public string DayName { get; set; }
            public int XpEarned { get; set; }
            public int HeightPercent { get; set; }
        }

        public async Task OnGetAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                StudentName = student.StudentName;
                ExpPoints = student.ExpPoints;
            }

            // Class Rank
            var allStudents = await _db.Students
                .OrderByDescending(s => s.ExpPoints)
                .ToListAsync();

            int rank = 1;
            foreach (var s in allStudents)
            {
                if (s.UserID == StudentID)
                {
                    ClassRank = rank;
                    break;
                }
                rank++;
            }

            // Learning progress per topic
            var progressList = await _db.LearningProgress
                .Include(lp => lp.Topic)
                .Where(lp => lp.StudentID == StudentID)
                .ToListAsync();

            if (progressList.Any())
            {
                TopicMasteryList = progressList.Select(lp => new TopicMasteryDto
                {
                    TopicName = lp.Topic != null ? lp.Topic.TopicName : "Topic",
                    MasteryPercentage = lp.ProgressPercentage
                }).ToList();
            }
            else
            {
                // Fallback default topics if student has not started
                TopicMasteryList = new List<TopicMasteryDto>
                {
                    new TopicMasteryDto { TopicName = "Algebra & Linear Equations", MasteryPercentage = 85 },
                    new TopicMasteryDto { TopicName = "Geometry Fundamentals", MasteryPercentage = 60 },
                    new TopicMasteryDto { TopicName = "Trigonometry & Ratios", MasteryPercentage = 45 }
                };
            }

            // Quiz History Log
            var userResults = await _db.QuizResults
                .Include(qr => qr.Quiz)
                .Where(qr => qr.StudentID == StudentID)
                .OrderByDescending(qr => qr.ResultID)
                .ToListAsync();

            QuizzesCompleted = userResults.Count;

            QuizHistoryList = userResults.Select(qr => new QuizHistoryDto
            {
                QuizTitle = qr.Quiz != null ? qr.Quiz.QuizTitle : "Math Quiz",
                TopicName = "Math Topics",
                Score = qr.Score,
                CorrectCount = qr.TotalCorrectAnswer,
                TotalCount = qr.TotalQuestionsAmount
            }).ToList();

            // Weekly XP Chart mock data
            WeeklyChartData = new List<DayXpDto>
            {
                new DayXpDto { DayName = "Mon", XpEarned = 30, HeightPercent = 40 },
                new DayXpDto { DayName = "Tue", XpEarned = 50, HeightPercent = 65 },
                new DayXpDto { DayName = "Wed", XpEarned = 80, HeightPercent = 100 },
                new DayXpDto { DayName = "Thu", XpEarned = 40, HeightPercent = 55 },
                new DayXpDto { DayName = "Fri", XpEarned = 60, HeightPercent = 75 },
                new DayXpDto { DayName = "Sat", XpEarned = 20, HeightPercent = 30 },
                new DayXpDto { DayName = "Sun", XpEarned = 70, HeightPercent = 85 }
            };
        }
    }
}
