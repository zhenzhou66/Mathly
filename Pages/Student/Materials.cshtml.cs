using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mathly.Pages.Student
{
    // Materials are now browsed per-topic via /Student/TopicDetail — this page
    // is kept only so old links/bookmarks to /app/materials don't dead-end.
    [Authorize(Roles = "student")]
    public class MaterialsModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Student/Topics");
    }
}
