using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    public class BadgesModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private const string StudentID = "student001"; // TODO: replace with session (#4)

        public BadgesModel(MathlyDbContext db) => _db = db;

        public int ExpPoints { get; set; }
        public int EarnedCount { get; set; }
        public int TotalCount { get; set; }
        public int NextUnlockExp { get; set; } = -1;
        public List<BadgeItem> EarnedBadges { get; set; } = new();
        public List<BadgeItem> LockedBadges { get; set; } = new();

        public class BadgeItem
        {
            public string BadgeID { get; set; }
            public string DisplayName { get; set; }
            public string Icon { get; set; }
            public int RequiredExp { get; set; }
            public DateOnly? EarnedDate { get; set; }
            public int ProgressPercent { get; set; }
        }

        private class BadgeDto
        {
            public string BadgeID { get; set; }
            public int ExpPoints { get; set; }
            public string ImageName { get; set; }
        }

        private class EarnedDto
        {
            public string BadgeID { get; set; }
            public DateOnly EarnedDate { get; set; }
        }

        public async Task OnGetAsync()
        {
            var student = await _db.Students.FindAsync(StudentID);
            ExpPoints = student?.ExpPoints ?? 0;

            var allBadges = await _db.Database
                .SqlQueryRaw<BadgeDto>(
                    "SELECT badgeID AS BadgeID, expPoints AS ExpPoints, CAST(badgeImage AS CHAR) AS ImageName FROM badges ORDER BY expPoints ASC")
                .ToListAsync();

            var earnedDtos = await _db.Database
                .SqlQueryRaw<EarnedDto>(
                    "SELECT badgeID AS BadgeID, earnedDate AS EarnedDate FROM studentbadges WHERE userID = {0}", StudentID)
                .ToListAsync();

            var earnedMap = earnedDtos.ToDictionary(e => e.BadgeID, e => e.EarnedDate);

            TotalCount = allBadges.Count;
            EarnedCount = earnedMap.Count;

            foreach (var b in allBadges)
            {
                var (name, icon) = FormatBadge(b.ImageName);
                var item = new BadgeItem
                {
                    BadgeID = b.BadgeID,
                    DisplayName = name,
                    Icon = icon,
                    RequiredExp = b.ExpPoints
                };

                if (earnedMap.TryGetValue(b.BadgeID, out var earnedDate))
                {
                    item.EarnedDate = earnedDate;
                    item.ProgressPercent = 100;
                    EarnedBadges.Add(item);
                }
                else
                {
                    item.ProgressPercent = b.ExpPoints > 0
                        ? Math.Min(100, (int)(ExpPoints * 100.0 / b.ExpPoints))
                        : 0;
                    LockedBadges.Add(item);
                }
            }

            var nextBadge = LockedBadges.OrderBy(b => b.RequiredExp).FirstOrDefault();
            NextUnlockExp = nextBadge != null ? nextBadge.RequiredExp - ExpPoints : -1;
        }

        private static (string Name, string Icon) FormatBadge(string imageFileName)
        {
            var baseName = (imageFileName ?? "").Split('.')[0].Replace("_", " ").Trim();
            var name = string.IsNullOrEmpty(baseName)
                ? "Badge"
                : System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(baseName);

            var icon = baseName.ToLower() switch
            {
                var s when s.Contains("bronze") => "🥉",
                var s when s.Contains("silver") => "🥈",
                var s when s.Contains("gold") => "🥇",
                var s when s.Contains("diamond") => "💎",
                _ => "🏅"
            };

            return (name, icon);
        }
    }
}
