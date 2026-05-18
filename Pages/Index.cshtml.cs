using Microsoft.AspNetCore.Mvc.RazorPages;
using Mathly.Data;
using Mathly.Models;
using Microsoft.EntityFrameworkCore;

namespace Mathly.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly MathlyDbContext _db;

        public IndexModel(MathlyDbContext db)
        {
            _db = db;
        }

        public List<StudentInfo> Students { get; set; }

        public async Task OnGetAsync()
        {
            Students = await _db.Students.ToListAsync();
        }
    }
}