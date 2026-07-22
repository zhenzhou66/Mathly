using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class LeaderboardModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public LeaderboardModel(MathlyDbContext db)
        {
            _db = db;
        }

        public string CurrentStudentID => StudentID;
        public int CurrentStudentRank { get; set; } = 0;
        public int CurrentStudentXP { get; set; } = 0;

        public LeaderboardStudentDto FirstPlace { get; set; }
        public LeaderboardStudentDto SecondPlace { get; set; }
        public LeaderboardStudentDto ThirdPlace { get; set; }

        public List<LeaderboardStudentDto> AllRankings { get; set; } = new();

        public class LeaderboardStudentDto
        {
            public int Rank { get; set; }
            public string UserID { get; set; }
            public string StudentName { get; set; }
            public string School { get; set; }
            public int ExpPoints { get; set; }
            public string AvatarChar { get; set; }
            public bool IsCurrentStudent { get; set; }
        }

        public async Task OnGetAsync()
        {
            var students = await _db.Students
                .OrderByDescending(s => s.ExpPoints)
                .ToListAsync();

            int rank = 1;
            foreach (var s in students)
            {
                var dto = new LeaderboardStudentDto
                {
                    Rank = rank,
                    UserID = s.UserID,
                    StudentName = s.StudentName ?? "Student",
                    School = string.IsNullOrEmpty(s.School) ? "Mathly Academy" : s.School,
                    ExpPoints = s.ExpPoints,
                    AvatarChar = !string.IsNullOrEmpty(s.StudentName) ? s.StudentName.Substring(0, 1).ToUpper() : "S",
                    IsCurrentStudent = s.UserID == StudentID
                };

                if (s.UserID == StudentID)
                {
                    CurrentStudentRank = rank;
                    CurrentStudentXP = s.ExpPoints;
                }

                AllRankings.Add(dto);
                rank++;
            }

            if (AllRankings.Count >= 1) FirstPlace = AllRankings[0];
            if (AllRankings.Count >= 2) SecondPlace = AllRankings[1];
            if (AllRankings.Count >= 3) ThirdPlace = AllRankings[2];
        }
    }
}
