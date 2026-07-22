using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;

namespace Mathly.Pages.Student
{
    [Authorize(Roles = "student")]
    public class AttemptQuizModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string StudentID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public AttemptQuizModel(MathlyDbContext db) => _db = db;

        // Bound from the combo boxes: name="Answers[<questionId>]" -> "A"/"B"/"C"/"D"
        [BindProperty]
        public Dictionary<string, string> Answers { get; set; } = new();

        public string QuizID { get; set; }
        public string QuizTitle { get; set; }
        public string TopicID { get; set; }
        public string ErrorMessage { get; set; }
        public string ValidationError { get; set; }
        public List<QuestionRow> Questions { get; set; } = new();

        // Only meaningful after a successful submission
        public bool Submitted { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public double Score { get; set; }

        public class QuestionRow
        {
            public string QuestionID { get; set; }
            public string QuestionText { get; set; }
            public string ChoiceA { get; set; }
            public string ChoiceB { get; set; }
            public string ChoiceC { get; set; }
            public string ChoiceD { get; set; }
            public string CorrectAnswer { get; set; }
            public string StudentAnswer { get; set; }
            public bool IsCorrect { get; set; }
        }

        private class QuestionDto
        {
            public string QuestionID { get; set; }
            public string QuestionText { get; set; }
            public string ChoiceA { get; set; }
            public string ChoiceB { get; set; }
            public string ChoiceC { get; set; }
            public string ChoiceD { get; set; }
            public string Answer { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string quizId)
        {
            if (!await LoadQuizAsync(quizId))
                return RedirectToPage("/Student/Topics");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string quizId)
        {
            if (!await LoadQuizAsync(quizId))
                return RedirectToPage("/Student/Topics");

            if (!Questions.Any())
                return Page();

            // Every question must have an answer selected before we grade/save anything.
            bool hasMissingAnswer = Questions.Any(q =>
                !Answers.TryGetValue(q.QuestionID, out var a) || string.IsNullOrEmpty(a));

            if (hasMissingAnswer)
            {
                ValidationError = "Please answer every question before submitting.";

                // Re-populate previously selected answers so the form doesn't reset.
                foreach (var q in Questions)
                {
                    Answers.TryGetValue(q.QuestionID, out var selected);
                    q.StudentAnswer = selected;
                }

                return Page();
            }

            int correctCount = 0;
            string firstAttemptId = null;

            // Next attempt number is computed once up front, then incremented
            // locally for each question — avoids a DB round trip per question.
            var nextAttemptNumber = await GetNextIdNumberAsync("quizstudentattempt", "attemptID", "att");

            foreach (var q in Questions)
            {
                Answers.TryGetValue(q.QuestionID, out var selected);

                bool isCorrect = !string.IsNullOrEmpty(selected)
                    && string.Equals(selected, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

                q.StudentAnswer = selected;
                q.IsCorrect = isCorrect;
                if (isCorrect) correctCount++;

                // quizstudentattempt's student-id column is `userID`, not `StudentID`
                // like the model property, so this is raw SQL rather than EF's
                // change tracker (same reason as the "Join topic" handler in Topics.cshtml.cs).
                var attemptId = $"att{nextAttemptNumber:D9}";
                nextAttemptNumber++;
                firstAttemptId ??= attemptId;

                await _db.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO quizstudentattempt (attemptID, quizID, userID, questionID, studentAnswer, isCorrect, attemptDuration)
                      VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    attemptId, quizId, StudentID, q.QuestionID, selected ?? "", isCorrect, 0);
            }

            TotalQuestions = Questions.Count;
            CorrectCount = correctCount;
            Score = Math.Round(100.0 * CorrectCount / TotalQuestions, 1);

            var resultNumber = await GetNextIdNumberAsync("quizresult", "resultID", "res");
            var resultId = $"res{resultNumber:D9}";

            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO quizresult (resultID, quizID, userID, attemptID, totalQuestionsAmount, totalCorrectAnswer, score)
                  VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                resultId, quizId, StudentID, firstAttemptId, TotalQuestions, CorrectCount, Score);

            Submitted = true;
            return Page();
        }

        // Looks at existing IDs like "res001" / "res000000005" in the given table/column,
        // strips the prefix, takes the highest numeric suffix, and returns the next number.
        // IDs that don't match "<prefix><digits>" (e.g. old GUID-style rows) are ignored.
        private async Task<int> GetNextIdNumberAsync(string table, string column, string prefix)
        {
            var maxNumber = await _db.Database
                .SqlQueryRaw<int>(
                    $@"SELECT COALESCE(MAX(CAST(SUBSTRING({column}, {prefix.Length + 1}) AS UNSIGNED)), 0) AS Value
                       FROM {table}
                       WHERE {column} REGEXP '^{prefix}[0-9]+$'")
                .FirstOrDefaultAsync();

            return maxNumber + 1;
        }

        private async Task<bool> LoadQuizAsync(string quizId)
        {
            if (string.IsNullOrWhiteSpace(quizId))
                return false;

            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.QuizID == quizId);
            if (quiz == null)
                return false;

            QuizID = quizId;
            QuizTitle = quiz.QuizTitle;
            TopicID = quiz.TopicID;

            // quizquestions' choice/answer columns don't match the model's property
            // names (questionChoiceA vs ChoiceA, questionAnswer vs Answer), so this
            // is raw SQL rather than a plain _db.QuizQuestions query.
            var dtos = await _db.Database
                .SqlQueryRaw<QuestionDto>(
                    @"SELECT questionID AS QuestionID, questionText AS QuestionText,
                             questionChoiceA AS ChoiceA, questionChoiceB AS ChoiceB,
                             questionChoiceC AS ChoiceC, questionChoiceD AS ChoiceD,
                             questionAnswer AS Answer
                      FROM quizquestions
                      WHERE quizID = {0}", quizId)
                .ToListAsync();

            Questions = dtos.Select(d => new QuestionRow
            {
                QuestionID = d.QuestionID,
                QuestionText = d.QuestionText,
                ChoiceA = d.ChoiceA,
                ChoiceB = d.ChoiceB,
                ChoiceC = d.ChoiceC,
                ChoiceD = d.ChoiceD,
                CorrectAnswer = d.Answer
            }).ToList();

            if (!Questions.Any())
                ErrorMessage = "This quiz doesn't have any questions yet.";

            return true;
        }
    }
}
