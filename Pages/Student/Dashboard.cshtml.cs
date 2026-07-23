using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class DashboardModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DashboardModel(MathlyDbContext db) => _db = db;

        public string StudentName { get; set; } = "Student";
        public int ExpPoints { get; set; }
        public int DayStreak { get; set; } = 5; // TODO: wire to streak tracking (#6), mock for now
        public int TopicsInProgress { get; set; }
        public int ClassRank { get; set; }
        public List<ProgressRow> ProgressList { get; set; } = new();
        public List<LeaderboardRow> Leaderboard { get; set; } = new();
        public List<BadgeRow> EarnedBadges { get; set; } = new();

        public class ProgressRow
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
            public double ProgressPercentage { get; set; }
        }

        public class LeaderboardRow
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public int ExpPoints { get; set; }
            public bool IsCurrentStudent { get; set; }
        }

        public class BadgeRow
        {
            public string BadgeName { get; set; }
            public int RequiredExp { get; set; }
            public DateOnly EarnedDate { get; set; }
        }

        private class LeaderboardDto
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public int ExpPoints { get; set; }
        }

        private class BadgeDto
        {
            public string BadgeName { get; set; }
            public int RequiredExp { get; set; }
            public DateOnly EarnedDate { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Get student name and XP
            var student = await _db.Students.FindAsync(StudentID);
            if (student != null)
            {
                StudentName = student.StudentName;
                ExpPoints = student.ExpPoints;
            }

            // Progress = quizzes the student has attempted / total quizzes for the topic,
            // scoped to topics the student has joined (studenttopic uses a `userID` column,
            // not `StudentID` like the model property, hence raw SQL).
            ProgressList = await _db.Database
                .SqlQueryRaw<ProgressRow>(
                    @"SELECT t.topicID AS TopicID, t.topicName AS TopicName,
                             CASE WHEN tot.TotalQuizzes > 0
                                  THEN ROUND(COALESCE(comp.CompletedQuizzes, 0) * 100.0 / tot.TotalQuizzes, 1)
                                  ELSE 0 END AS ProgressPercentage
                      FROM studenttopic st
                      JOIN topic t ON st.topicID = t.topicID
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
                .ToListAsync();

            TopicsInProgress = ProgressList.Count(p => p.ProgressPercentage < 100);

            // Raw SQL: leaderboard (top 5 students by XP)
            var leaderboardDtos = await _db.Database
                .SqlQueryRaw<LeaderboardDto>(
                    "SELECT userID AS StudentID, studentName AS StudentName, expPoints AS ExpPoints FROM studentinfo ORDER BY expPoints DESC LIMIT 5")
                .ToListAsync();

            Leaderboard = leaderboardDtos
                .Select(l => new LeaderboardRow
                {
                    StudentID = l.StudentID,
                    StudentName = l.StudentName,
                    ExpPoints = l.ExpPoints,
                    IsCurrentStudent = l.StudentID == StudentID
                })
                .ToList();

            // Raw SQL: class rank by XP
            ClassRank = 1 + await _db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM studentinfo WHERE expPoints > (SELECT expPoints FROM studentinfo WHERE userID = {0})", StudentID)
                .FirstOrDefaultAsync();

            // Raw SQL: earned badges (studentbadges/badges use userID/badgeID columns; badgeImage stores the icon filename as text)
            EarnedBadges = (await _db.Database
                .SqlQueryRaw<BadgeDto>(
                    @"SELECT CAST(b.badgeImage AS CHAR) AS BadgeName, b.expPoints AS RequiredExp, sb.earnedDate AS EarnedDate
                      FROM studentbadges sb
                      JOIN badges b ON sb.badgeID = b.badgeID
                      WHERE sb.userID = {0}
                      ORDER BY sb.earnedDate DESC", StudentID)
                .ToListAsync())
                .Select(b => new BadgeRow
                {
                    BadgeName = b.BadgeName,
                    RequiredExp = b.RequiredExp,
                    EarnedDate = b.EarnedDate
                }).ToList();
        }
    }
}
