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
    public class DailyModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DailyModel(MathlyDbContext db)
        {
            _db = db;
        }

        public string StudentName { get; set; } = "Student";
        public string DailyQuizID { get; set; } = "quiz001";
        public string DailyQuizTitle { get; set; } = "Daily Math Challenge";
        public string TopicName { get; set; } = "Algebra & Functions";
        public int TotalQuestions { get; set; } = 5;
        public int ExpPointsReward { get; set; } = 50;
        public int EstimatedMinutes { get; set; } = 10;
        public int ParticipantsTodayCount { get; set; } = 0;

        public bool HasCompletedToday { get; set; } = false;
        public double UserTodayScore { get; set; } = 0;
        public int UserTodayCorrect { get; set; } = 0;

        public List<ParticipantDto> TodayParticipants { get; set; } = new();

        public class ParticipantDto
        {
            public string StudentName { get; set; }
            public double Score { get; set; }
            public string AvatarChar { get; set; }
        }

        private class CountDto
        {
            public int Value { get; set; }
        }

        private class TakerDto
        {
            public string StudentName { get; set; }
            public double Score { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                StudentName = student.StudentName;
            }

            // Pick a daily quiz (or fallback to first quiz available)
            var quiz = await _db.Quizzes.FirstOrDefaultAsync();
            if (quiz != null)
            {
                DailyQuizID = quiz.QuizID;
                DailyQuizTitle = quiz.QuizTitle;

                if (!string.IsNullOrEmpty(quiz.TopicID))
                {
                    var topic = await _db.Topics.FindAsync(quiz.TopicID);
                    if (topic != null)
                    {
                        TopicName = topic.TopicName;
                    }
                }

                TotalQuestions = await _db.QuizQuestions.CountAsync(q => q.QuizID == quiz.QuizID);
                if (TotalQuestions == 0) TotalQuestions = 5;
            }

            // Check if student has taken this daily quiz
            var existingResult = await _db.QuizResults
                .Where(r => r.StudentID == StudentID && r.QuizID == DailyQuizID)
                .OrderByDescending(r => r.ResultID)
                .FirstOrDefaultAsync();

            if (existingResult != null)
            {
                HasCompletedToday = true;
                UserTodayScore = existingResult.Score;
                UserTodayCorrect = existingResult.TotalCorrectAnswer;
            }

            // Count participants
            var pCount = await _db.QuizResults
                .Where(r => r.QuizID == DailyQuizID)
                .Select(r => r.StudentID)
                .Distinct()
                .CountAsync();
            ParticipantsTodayCount = pCount;

            // Fetch recent takers
            var recentTakers = await _db.Database
                .SqlQueryRaw<TakerDto>(
                    @"SELECT si.studentName AS StudentName, qr.score AS Score
                      FROM quizresult qr
                      JOIN studentinfo si ON qr.userID = si.userID
                      WHERE qr.quizID = {0}
                      ORDER BY qr.resultID DESC
                      LIMIT 5", DailyQuizID)
                .ToListAsync();

            TodayParticipants = recentTakers.Select(t => new ParticipantDto
            {
                StudentName = t.StudentName,
                Score = t.Score,
                AvatarChar = !string.IsNullOrEmpty(t.StudentName) ? t.StudentName.Substring(0, 1).ToUpper() : "S"
            }).ToList();

            return Page();
        }
    }
}
