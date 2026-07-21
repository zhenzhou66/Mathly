using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class QuizzesModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public QuizzesModel(MathlyDbContext db) => _db = db;

        public List<QuizRow> Quizzes { get; set; } = new();

        public class QuizRow
        {
            public string QuizID { get; set; }
            public string QuizTitle { get; set; }
            public string TopicName { get; set; }
            public double? BestScore { get; set; }
        }

        public async Task OnGetAsync()
        {
            // studenttopic/quizresult use `userID` as the student-id column, not
            // `studentID`, so this is raw SQL rather than EF navigation properties.
            Quizzes = (await _db.Database
                .SqlQueryRaw<QuizRow>(
                    @"SELECT q.quizID AS QuizID, q.quizTitle AS QuizTitle, t.topicName AS TopicName, best.BestScore AS BestScore
                      FROM studenttopic st
                      JOIN topic t ON st.topicID = t.topicID
                      JOIN quizzes q ON q.topicID = t.topicID
                      LEFT JOIN (
                          SELECT quizID, userID, MAX(score) AS BestScore
                          FROM quizresult
                          GROUP BY quizID, userID
                      ) best ON best.quizID = q.quizID AND best.userID = st.userID
                      WHERE st.userID = {0}
                      ORDER BY t.topicName, q.quizTitle", StudentID)
                .ToListAsync());
        }
    }
}
