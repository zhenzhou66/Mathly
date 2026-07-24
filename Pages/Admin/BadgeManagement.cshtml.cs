using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Admin
{
    public class BadgeManagementModel : PageModel
    {
        private readonly MathlyDbContext _db;
        public BadgeManagementModel(MathlyDbContext db) => _db = db;

        public List<BadgeRow> Badges { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public readonly string[] PresetIcons = new[]
        {
            "bi-trophy-fill",
            "bi-award-fill",
            "bi-mortarboard-fill",
            "bi-gem",
            "bi-patch-check-fill"
        };

        public class BadgeRow
        {
            public string BadgeID { get; set; } = "";
            public string BadgeName { get; set; } = "";
            public int ExpPoints { get; set; }
            public string IconClass { get; set; } = "bi-trophy-fill";
            public int StudentsEarned { get; set; }
        }

        [BindProperty] public string NewBadgeName { get; set; } = "";
        [BindProperty] public int NewExpPoints { get; set; }
        [BindProperty] public string NewIconClass { get; set; } = "bi-trophy-fill";

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadBadgesAsync();
        }

        private async Task LoadBadgesAsync()
        {
            var badges = await _db.Badges.ToListAsync();
            var studentBadges = await _db.StudentBadges.ToListAsync();

            Badges = badges.Select(b => new BadgeRow
            {
                BadgeID = b.BadgeID,
                BadgeName = b.BadgeName,
                ExpPoints = b.ExpPoints,
                IconClass = string.IsNullOrEmpty(b.IconClass) ? "bi-trophy-fill" : b.IconClass,
                StudentsEarned = studentBadges.Count(sb => sb.BadgeID == b.BadgeID)
            })
            .OrderBy(b => b.ExpPoints)
            .ToList();
        }

        public async Task<IActionResult> OnPostAddBadgeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewBadgeName))
            {
                TempData["Error"] = "Badge name is required.";
                return RedirectToPage();
            }

            var count = await _db.Badges.CountAsync();
            var newId = $"badge{(count + 1):D3}";
            while (await _db.Badges.FindAsync(newId) != null)
                newId += "x";

            _db.Badges.Add(new Badges
            {
                BadgeID = newId,
                BadgeName = NewBadgeName,
                ExpPoints = NewExpPoints,
                IconClass = NewIconClass
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Badge '{NewBadgeName}' created.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditBadgeAsync(
            string badgeId, string badgeName, int expPoints, string iconClass)
        {
            var badge = await _db.Badges.FindAsync(badgeId);
            if (badge == null) return RedirectToPage();

            if (string.IsNullOrWhiteSpace(badgeName))
            {
                TempData["Error"] = "Badge name is required.";
                return RedirectToPage();
            }

            badge.BadgeName = badgeName;
            badge.ExpPoints = expPoints;
            badge.IconClass = iconClass;

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Badge '{badgeName}' updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteBadgeAsync(string badgeId)
        {
            var hasBeenEarned = await _db.StudentBadges.AnyAsync(sb => sb.BadgeID == badgeId);
            if (hasBeenEarned)
            {
                TempData["Error"] = "Cannot delete a badge that students have already earned.";
                return RedirectToPage();
            }

            var badge = await _db.Badges.FindAsync(badgeId);
            if (badge != null)
            {
                _db.Badges.Remove(badge);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Badge deleted.";
            }
            return RedirectToPage();
        }
    }
}