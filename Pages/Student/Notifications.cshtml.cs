using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Student
{
    public class NotificationsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private const string StudentID = "student001"; // TODO: replace with session (#4)

        public NotificationsModel(MathlyDbContext db) => _db = db;

        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }

        public async Task OnGetAsync()
        {
            Notifications = await _db.Notifications
                .Where(n => n.UserID == StudentID)
                .OrderByDescending(n => n.NotificationID)
                .ToListAsync();

            UnreadCount = Notifications.Count(n => !n.IsRead);
        }

        public async Task<IActionResult> OnPostMarkReadAsync(string id)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == StudentID);
            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var unread = await _db.Notifications
                .Where(n => n.UserID == StudentID && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public static string IconFor(string type) => type switch
        {
            "badge" => "🏅",
            "material" => "📄",
            "progress" => "📈",
            "quiz" => "📝",
            "activity" => "👀",
            "system" => "🔔",
            _ => "🔔"
        };
    }
}
