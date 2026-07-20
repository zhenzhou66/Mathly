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

        private class ProgressDto
        {
            public string TopicID { get; set; }
            public double ProgressPercentage { get; set; }
        }

        private class TopicNameDto
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
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

            // Raw SQL: learning progress for this student (learningprogress uses userID column)
            var progressDtos = await _db.Database
                .SqlQueryRaw<ProgressDto>(
                    "SELECT topicID AS TopicID, progressPercentage AS ProgressPercentage FROM learningprogress WHERE userID = {0}", StudentID)
                .ToListAsync();

            TopicsInProgress = progressDtos.Count(p => p.ProgressPercentage < 100);

            if (progressDtos.Any())
            {
                var topicIds = progressDtos.Select(p => p.TopicID).ToList();
                var topicNames = await _db.Topics
                    .Where(t => topicIds.Contains(t.TopicID))
                    .Select(t => new TopicNameDto { TopicID = t.TopicID, TopicName = t.TopicName })
                    .ToListAsync();

                ProgressList = progressDtos
                    .Join(topicNames, p => p.TopicID, t => t.TopicID, (p, t) => new ProgressRow
                    {
                        TopicName = t.TopicName,
                        ProgressPercentage = p.ProgressPercentage
                    })
                    .ToList();
            }

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
