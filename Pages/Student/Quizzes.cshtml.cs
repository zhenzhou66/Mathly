using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mathly.Pages.Student
{
    // Quizzes are now browsed per-topic via /Student/TopicDetail — this page
    // is kept only so old links/bookmarks to /app/quizzes don't dead-end.
    [Authorize(Roles = "student")]
    public class QuizzesModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Student/Topics");
    }
}
