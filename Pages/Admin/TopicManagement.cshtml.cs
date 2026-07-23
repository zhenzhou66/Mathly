using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Admin
{
    public class TopicManagementModel : PageModel
    {
        private readonly MathlyDbContext _db;
        public TopicManagementModel(MathlyDbContext db) => _db = db;

        public List<TopicRow> Topics { get; set; } = new();
        public List<TeacherOption> AllTeachers { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class TopicRow
        {
            public string TopicID { get; set; } = "";
            public string TopicName { get; set; } = "";
            public string TeacherID { get; set; }
            public string TeacherName { get; set; } = "";
            public int QuizCount { get; set; }
            public int StudentCount { get; set; }
        }

        public class TeacherOption
        {
            public string UserID { get; set; } = "";
            public string TeacherName { get; set; } = "";
        }

        [BindProperty] public string NewTopicName { get; set; } = "";
        [BindProperty] public string NewTeacherId { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"] as string;
            ErrorMessage = TempData["Error"] as string;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var topics = await _db.Topics.ToListAsync();
            var teachers = await _db.Teachers.ToListAsync();
            var quizzes = await _db.Quizzes.ToListAsync();
            var studentTopics = await _db.StudentTopics.ToListAsync();

            AllTeachers = teachers
                .Select(t => new TeacherOption { UserID = t.UserID, TeacherName = t.TeacherName })
                .OrderBy(t => t.TeacherName)
                .ToList();

            Topics = topics.Select(t =>
            {
                var teacher = teachers.FirstOrDefault(x => x.UserID == t.TeacherID);
                return new TopicRow
                {
                    TopicID = t.TopicID,
                    TopicName = t.TopicName,
                    TeacherID = t.TeacherID,
                    TeacherName = teacher?.TeacherName ?? "Unassigned",
                    QuizCount = quizzes.Count(q => q.TopicID == t.TopicID),
                    StudentCount = studentTopics.Count(st => st.TopicID == t.TopicID)
                };
            })
            .OrderBy(t => t.TopicName)
            .ToList();
        }

        public async Task<IActionResult> OnPostAddTopicAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTopicName))
            {
                TempData["Error"] = "Topic name is required.";
                return RedirectToPage();
            }

            var count = await _db.Topics.CountAsync();
            var newId = $"topic{(count + 1):D3}";
            while (await _db.Topics.FindAsync(newId) != null)
                newId += "x";

            _db.Topics.Add(new Topic
            {
                TopicID = newId,
                TopicName = NewTopicName,
                TeacherID = string.IsNullOrWhiteSpace(NewTeacherId) ? null : NewTeacherId
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Topic '{NewTopicName}' created.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditTopicAsync(string topicId, string topicName, string? teacherId)
        {
            var topic = await _db.Topics.FindAsync(topicId);
            if (topic != null)
            {
                topic.TopicName = topicName;
                topic.TeacherID = string.IsNullOrWhiteSpace(teacherId) ? null : teacherId;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Topic '{topicName}' updated.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTopicAsync(string topicId)
        {
            var hasQuizzes = await _db.Quizzes.AnyAsync(q => q.TopicID == topicId);
            var hasStudyMaterial = await _db.StudyMaterials.AnyAsync(m => m.TopicID == topicId);
            var hasProgress = await _db.LearningProgress.AnyAsync(p => p.TopicID == topicId);
            var hasStudentTopics = await _db.StudentTopics.AnyAsync(st => st.TopicID == topicId);

            if (hasQuizzes || hasStudyMaterial || hasProgress || hasStudentTopics)
            {
                TempData["Error"] = "Cannot delete a topic that has quizzes, materials, progress, or enrolled students. Remove those first.";
                return RedirectToPage();
            }

            var topic = await _db.Topics.FindAsync(topicId);
            if (topic != null)
            {
                _db.Topics.Remove(topic);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Topic deleted.";
            }
            return RedirectToPage();
        }
    }
}