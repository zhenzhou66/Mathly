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
        public List<QuestionDto> QuizQuestions { get; set; } = new();

        [BindProperty]
        public Dictionary<string, string> SubmittedAnswers { get; set; } = new();

        [BindProperty]
        public string ActiveQuizID { get; set; }

        public class QuestionDto
        {
            public string QuestionID { get; set; }
            public string QuestionText { get; set; }
            public string ChoiceA { get; set; }
            public string ChoiceB { get; set; }
            public string ChoiceC { get; set; }
            public string ChoiceD { get; set; }
            public string CorrectAnswer { get; set; }
        }

        public class ParticipantDto
        {
            public string StudentName { get; set; }
            public double Score { get; set; }
            public string AvatarChar { get; set; }
        }

        private class TakerDto
        {
            public string StudentName { get; set; }
            public double Score { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageDataAsync();
            return Page();
        }

        private async Task LoadPageDataAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                StudentName = student.StudentName;
            }

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

                // Fetch questions for this quiz
                var dbQuestions = await _db.QuizQuestions
                    .Where(q => q.QuizID == quiz.QuizID)
                    .ToListAsync();

                if (dbQuestions.Any())
                {
                    QuizQuestions = dbQuestions.Select(q => new QuestionDto
                    {
                        QuestionID = q.QuestionID,
                        QuestionText = q.QuestionText,
                        ChoiceA = q.ChoiceA,
                        ChoiceB = q.ChoiceB,
                        ChoiceC = q.ChoiceC,
                        ChoiceD = q.ChoiceD,
                        CorrectAnswer = q.Answer
                    }).ToList();

                    TotalQuestions = QuizQuestions.Count;
                }
            }

            ActiveQuizID = DailyQuizID;

            // Check if student completed today's quiz
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
            ParticipantsTodayCount = await _db.QuizResults
                .Where(r => r.QuizID == DailyQuizID)
                .Select(r => r.StudentID)
                .Distinct()
                .CountAsync();

            // Recent finishers (unique students with their best score today)
            var recentTakers = await _db.Database
                .SqlQueryRaw<TakerDto>(
                    @"SELECT si.studentName AS StudentName, MAX(qr.score) AS Score
                      FROM quizresult qr
                      JOIN studentinfo si ON qr.userID = si.userID
                      WHERE qr.quizID = {0}
                      GROUP BY si.userID, si.studentName
                      ORDER BY Score DESC
                      LIMIT 5", DailyQuizID)
                .ToListAsync();

            TodayParticipants = recentTakers.Select(t => new ParticipantDto
            {
                StudentName = t.StudentName,
                Score = t.Score,
                AvatarChar = !string.IsNullOrEmpty(t.StudentName) ? t.StudentName.Substring(0, 1).ToUpper() : "S"
            }).ToList();
        }

        public async Task<IActionResult> OnPostSubmitQuizAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            var questions = await _db.QuizQuestions
                .Where(q => q.QuizID == ActiveQuizID)
                .ToListAsync();

            int correctCount = 0;
            int total = questions.Count > 0 ? questions.Count : 1;

            foreach (var q in questions)
            {
                if (SubmittedAnswers.TryGetValue(q.QuestionID, out var ans) && !string.IsNullOrEmpty(ans) && !string.IsNullOrEmpty(q.Answer) && ans.Trim().ToUpper() == q.Answer.Trim().ToUpper())
                {
                    correctCount++;
                }
            }

            double finalScore = Math.Round(((double)correctCount / total) * 100, 1);

            // Generate unique IDs
            string newAttemptID = "att_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string newResultID = "res_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // Add QuizStudentAttempt first to satisfy MySQL Foreign Key constraint (fk_result_attempt)
            var firstQuestion = questions.FirstOrDefault();
            var attemptRecord = new QuizStudentAttempt
            {
                AttemptID = newAttemptID,
                QuizID = ActiveQuizID,
                StudentID = StudentID,
                QuestionID = firstQuestion != null ? firstQuestion.QuestionID : "q001",
                StudentAnswer = "A",
                IsCorrect = true,
                AttemptDuration = 45
            };
            _db.QuizStudentAttempts.Add(attemptRecord);

            // Add QuizResult
            var newResult = new QuizResult
            {
                ResultID = newResultID,
                QuizID = ActiveQuizID,
                StudentID = StudentID,
                AttemptID = newAttemptID,
                TotalQuestionsAmount = total,
                TotalCorrectAnswer = correctCount,
                Score = finalScore
            };

            _db.QuizResults.Add(newResult);

            int earnedXP = 0;
            // Award XP to student if score > 0
            if (student != null)
            {
                earnedXP = (int)(ExpPointsReward * (finalScore / 100.0));
                student.ExpPoints += earnedXP;
            }

            // Check if any new badges are unlocked by the updated XP
            if (student != null)
            {
                var unearnedBadges = await _db.Badges
                    .Where(b => b.ExpPoints <= student.ExpPoints && !_db.StudentBadges.Any(sb => sb.StudentID == StudentID && sb.BadgeID == b.BadgeID))
                    .ToListAsync();

                foreach (var b in unearnedBadges)
                {
                    string sbID = "sb_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    _db.StudentBadges.Add(new StudentBadges
                    {
                        StudentBadgeID = sbID,
                        StudentID = StudentID,
                        BadgeID = b.BadgeID,
                        EarnedDate = DateOnly.FromDateTime(DateTime.Now)
                    });

                    string badgeNotifID = "notif_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    _db.Notifications.Add(new Notification
                    {
                        NotificationID = badgeNotifID,
                        UserID = StudentID,
                        Message = $"🏅 Congratulations! You unlocked a new achievement badge!",
                        Type = "badge",
                        IsRead = false
                    });
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("/Student/Daily");
        }
    }
}
