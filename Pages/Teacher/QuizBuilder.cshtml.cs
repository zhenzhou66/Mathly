using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class QuizBuilderModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public QuizBuilderModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";
        public string Message { get; set; } = "";
        public List<Topic> TeacherTopics { get; set; } = new();
        public List<RecentQuiz> RecentQuizzes { get; set; } = new();

        [BindProperty]
        public QuizInput Quiz { get; set; } = new();

        [BindProperty]
        public List<QuestionInput> Questions { get; set; } = new();

        public class QuizInput
        {
            public string Title { get; set; } = "";
            public string TopicId { get; set; } = "";
            public string Difficulty { get; set; } = "Medium";
            public int TimeLimit { get; set; } = 20;
            public int QuestionCount { get; set; } = 3;
            public string Instructions { get; set; } = "";
        }

        public class QuestionInput
        {
            public string QuestionText { get; set; } = "";
            public string ChoiceA { get; set; } = "";
            public string ChoiceB { get; set; } = "";
            public string ChoiceC { get; set; } = "";
            public string ChoiceD { get; set; } = "";
            public string CorrectAnswer { get; set; } = "A";
        }

        public class RecentQuiz
        {
            public string Title { get; set; } = "";
            public string TopicName { get; set; } = "";
            public int QuestionCount { get; set; }
        }

        public async Task OnGetAsync(string? topicId)
        {
            await LoadAsync();
            if (!string.IsNullOrWhiteSpace(topicId))
                Quiz.TopicId = topicId;
        }

        public async Task<IActionResult> OnPostAddQuestionAsync()
        {
            Questions.Add(new QuestionInput());
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveQuizAsync()
        {
            if (string.IsNullOrWhiteSpace(Quiz.Title))
            {
                Message = "Please add a quiz title.";
                await LoadAsync();
                return Page();
            }

            var topic = await _db.Topics.FirstOrDefaultAsync(t => t.TopicID == Quiz.TopicId && t.TeacherID == TeacherID);
            if (topic == null)
            {
                Message = "Please choose a valid topic.";
                await LoadAsync();
                return Page();
            }

            var quiz = new Quizzes
            {
                QuizID = Guid.NewGuid().ToString("N"),
                TopicID = topic.TopicID,
                QuizTitle = Quiz.Title.Trim()
            };

            _db.Quizzes.Add(quiz);
            await _db.SaveChangesAsync();

            var questionCount = 0;
            foreach (var question in Questions.Where(q => !string.IsNullOrWhiteSpace(q.QuestionText)))
            {
                _db.QuizQuestions.Add(new QuizQuestions
                {
                    QuestionID = Guid.NewGuid().ToString("N"),
                    QuizID = quiz.QuizID,
                    QuestionText = question.QuestionText.Trim(),
                    ChoiceA = question.ChoiceA.Trim(),
                    ChoiceB = question.ChoiceB.Trim(),
                    ChoiceC = question.ChoiceC.Trim(),
                    ChoiceD = question.ChoiceD.Trim(),
                    Answer = (question.CorrectAnswer ?? "A").Substring(0, 1).ToUpper()
                });
                questionCount++;
            }

            await _db.SaveChangesAsync();
            Message = $"Quiz '{quiz.QuizTitle}' published with {questionCount} questions.";
            Quiz = new QuizInput();
            Questions = new List<QuestionInput> { new(), new(), new() };
            await LoadAsync();
            return Page();
        }

        private async Task LoadAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
                TeacherName = teacher.TeacherName;

            TeacherTopics = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .OrderBy(t => t.TopicName)
                .ToListAsync();

            Questions ??= new List<QuestionInput>();
            if (Questions.Count == 0)
                Questions = new List<QuestionInput> { new(), new(), new() };

            var topicIds = TeacherTopics.Select(t => t.TopicID).ToList();
            RecentQuizzes = await (from q in _db.Quizzes
                                   join t in _db.Topics on q.TopicID equals t.TopicID
                                   where topicIds.Contains(q.TopicID)
                                   orderby q.QuizID descending
                                   select new RecentQuiz
                                   {
                                       Title = q.QuizTitle,
                                       TopicName = t.TopicName,
                                       QuestionCount = _db.QuizQuestions.Count(qq => qq.QuizID == q.QuizID)
                                   })
                                   .Take(5)
                                   .ToListAsync();
        }
    }
}
