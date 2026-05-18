using Microsoft.AspNetCore.Mvc.RazorPages;
using testWebApp.Data;
using testWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace testWebApp.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly testDbContext _db;

        public IndexModel(testDbContext db)
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