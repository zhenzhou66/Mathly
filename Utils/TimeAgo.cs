namespace Mathly.Utils
{
    // Small shared helper so Discussion.cshtml.cs and DiscussionDetail.cshtml.cs
    // don't each reimplement "how long ago was this posted" formatting.
    public static class TimeAgo
    {
        public static string Format(DateTime postedDate)
        {
            var span = DateTime.Now - postedDate;

            if (span.TotalSeconds < 60) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";

            return postedDate.ToString("d MMM yyyy");
        }
    }
}
