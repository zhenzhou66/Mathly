using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class MaterialsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public MaterialsModel(MathlyDbContext db) => _db = db;

        public List<MaterialRow> Materials { get; set; } = new();

        public class MaterialRow
        {
            public string MaterialID { get; set; }
            public string FileName { get; set; }
            public string TopicName { get; set; }
        }

        public async Task OnGetAsync()
        {
            // studenttopic's student-id column is `userID`, and studymaterial.fileName
            // is stored as a blob, so both need raw SQL (CAST(...) mirrors the pattern
            // already used for badges.badgeImage in Student/Dashboard.cshtml.cs).
            Materials = (await _db.Database
                .SqlQueryRaw<MaterialRow>(
                    @"SELECT sm.materialID AS MaterialID, CAST(sm.fileName AS CHAR) AS FileName, t.topicName AS TopicName
                      FROM studenttopic st
                      JOIN topic t ON st.topicID = t.topicID
                      JOIN studymaterial sm ON sm.topicID = t.topicID
                      WHERE st.userID = {0}
                      ORDER BY t.topicName, sm.fileName", StudentID)
                .ToListAsync());
        }
    }
}
