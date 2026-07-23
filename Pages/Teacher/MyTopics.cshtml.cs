using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class MyTopicsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public MyTopicsModel(MathlyDbContext db) => _db = db;

        public string TeacherName { get; set; } = "Teacher";
        public string Message { get; set; } = "";
        public string ActiveTopicId { get; set; } = "";
        public string ActiveTopicTitle { get; set; } = "";
        public List<TopicSummary> Topics { get; set; } = new();
        public List<ChapterSummary> Chapters { get; set; } = new();

        [BindProperty]
        public TopicFormInput TopicForm { get; set; } = new();

        [BindProperty]
        public ChapterFormInput ChapterForm { get; set; } = new();

        public class TopicSummary
        {
            public string TopicId { get; set; } = "";
            public string Title { get; set; } = "";
            public int QuizCount { get; set; }
            public int ChapterCount { get; set; }
            public int StudentCount { get; set; }
        }

        public class ChapterSummary
        {
            public string ChapterId { get; set; } = "";
            public string Title { get; set; } = "";
            public string Goal { get; set; } = "";
            public string Content { get; set; } = "";
            public int Order { get; set; }
        }

        public class TopicFormInput
        {
            public string TopicId { get; set; } = "";
            public string TopicName { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class ChapterFormInput
        {
            public string TopicId { get; set; } = "";
            public string ChapterId { get; set; } = "";
            public string Title { get; set; } = "";
            public string Goal { get; set; } = "";
            public string Content { get; set; } = "";
            public int Order { get; set; } = 1;
        }

        public async Task OnGetAsync(string? topicId, string? chapterId)
        {
            await LoadAsync(topicId, chapterId);
        }

        public async Task<IActionResult> OnPostSaveTopicAsync()
        {
            if (string.IsNullOrWhiteSpace(TopicForm.TopicName))
            {
                Message = "Please enter a topic title.";
                await LoadAsync(null, null);
                return Page();
            }

            Topic topic;
            if (!string.IsNullOrWhiteSpace(TopicForm.TopicId))
            {
                topic = await _db.Topics.FirstOrDefaultAsync(t => t.TopicID == TopicForm.TopicId && t.TeacherID == TeacherID);
                if (topic == null)
                {
                    Message = "The selected topic could not be found.";
                    await LoadAsync(null, null);
                    return Page();
                }

                topic.TopicName = TopicForm.TopicName.Trim();
            }
            else
            {
                topic = new Topic
                {
                    TopicID = Guid.NewGuid().ToString("N"),
                    TeacherID = TeacherID,
                    TopicName = TopicForm.TopicName.Trim()
                };
                _db.Topics.Add(topic);
            }

            await _db.SaveChangesAsync();
            Message = string.IsNullOrWhiteSpace(TopicForm.TopicId) ? "Topic created successfully." : "Topic updated successfully.";
            await LoadAsync(topic.TopicID, null);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveChapterAsync()
        {
            if (string.IsNullOrWhiteSpace(ChapterForm.Title) || string.IsNullOrWhiteSpace(ChapterForm.Content))
            {
                Message = "Please complete the chapter title and content before saving.";
                await LoadAsync(ChapterForm.TopicId, null);
                return Page();
            }

            var topic = await _db.Topics.FirstOrDefaultAsync(t => t.TopicID == ChapterForm.TopicId && t.TeacherID == TeacherID);
            if (topic == null)
            {
                Message = "Please select a valid topic before adding a chapter.";
                await LoadAsync(null, null);
                return Page();
            }

            var payload = new
            {
                Title = ChapterForm.Title.Trim(),
                Goal = ChapterForm.Goal.Trim(),
                Content = ChapterForm.Content.Trim(),
                Order = ChapterForm.Order
            };

            if (!string.IsNullOrWhiteSpace(ChapterForm.ChapterId))
            {
                var existing = await _db.StudyMaterials.FirstOrDefaultAsync(m => m.MaterialID == ChapterForm.ChapterId && m.TopicID == topic.TopicID);
                if (existing != null)
                {
                    existing.FileName = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
                }
            }
            else
            {
                _db.StudyMaterials.Add(new StudyMaterial
                {
                    MaterialID = Guid.NewGuid().ToString("N"),
                    TopicID = topic.TopicID,
                    FileName = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))
                });
            }

            await _db.SaveChangesAsync();
            Message = "Chapter saved successfully.";
            await LoadAsync(topic.TopicID, null);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteChapterAsync(string chapterId)
        {
            var material = await _db.StudyMaterials.FirstOrDefaultAsync(m => m.MaterialID == chapterId);
            if (material != null)
            {
                _db.StudyMaterials.Remove(material);
                await _db.SaveChangesAsync();
                Message = "Chapter removed.";
            }

            await LoadAsync(null, null);
            return Page();
        }

        private async Task LoadAsync(string? selectedTopicId, string? selectedChapterId)
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
                TeacherName = teacher.TeacherName;

            var teacherTopics = await _db.Topics
                .Where(t => t.TeacherID == TeacherID)
                .OrderBy(t => t.TopicName)
                .ToListAsync();

            Topics = new List<TopicSummary>();
            foreach (var topic in teacherTopics)
            {
                var quizCount = await _db.Quizzes.CountAsync(q => q.TopicID == topic.TopicID);
                var chapterCount = await _db.StudyMaterials.CountAsync(m => m.TopicID == topic.TopicID);
                var studentCount = await _db.StudentTopics.CountAsync(s => s.TopicID == topic.TopicID);
                Topics.Add(new TopicSummary
                {
                    TopicId = topic.TopicID,
                    Title = topic.TopicName,
                    QuizCount = quizCount,
                    ChapterCount = chapterCount,
                    StudentCount = studentCount
                });
            }

            if (string.IsNullOrWhiteSpace(selectedTopicId) && Topics.Any())
                selectedTopicId = Topics.First().TopicId;

            if (!string.IsNullOrWhiteSpace(selectedTopicId))
            {
                var selectedTopic = teacherTopics.FirstOrDefault(t => t.TopicID == selectedTopicId);
                if (selectedTopic != null)
                {
                    ActiveTopicId = selectedTopic.TopicID;
                    ActiveTopicTitle = selectedTopic.TopicName;
                    TopicForm = new TopicFormInput
                    {
                        TopicId = selectedTopic.TopicID,
                        TopicName = selectedTopic.TopicName
                    };

                    var rawMaterials = await _db.StudyMaterials
                        .Where(m => m.TopicID == selectedTopic.TopicID)
                        .ToListAsync();

                    Chapters = new List<ChapterSummary>();
                    foreach (var material in rawMaterials)
                    {
                        try
                        {
                            var json = Encoding.UTF8.GetString(material.FileName);
                            var payload = JsonSerializer.Deserialize<ChapterPayload>(json);
                            if (payload is not null)
                            {
                                Chapters.Add(new ChapterSummary
                                {
                                    ChapterId = material.MaterialID,
                                    Title = payload.Title,
                                    Goal = payload.Goal,
                                    Content = payload.Content,
                                    Order = payload.Order
                                });
                                continue;
                            }
                        }
                        catch
                        {
                            // ignore malformed stored content
                        }

                        Chapters.Add(new ChapterSummary
                        {
                            ChapterId = material.MaterialID,
                            Title = Encoding.UTF8.GetString(material.FileName),
                            Goal = "Lesson note",
                            Content = Encoding.UTF8.GetString(material.FileName),
                            Order = 1
                        });
                    }

                    Chapters = Chapters.OrderBy(c => c.Order).ThenBy(c => c.Title).ToList();

                    if (!string.IsNullOrWhiteSpace(selectedChapterId))
                    {
                        var selectedChapter = Chapters.FirstOrDefault(c => c.ChapterId == selectedChapterId);
                        if (selectedChapter != null)
                        {
                            ChapterForm = new ChapterFormInput
                            {
                                TopicId = selectedTopic.TopicID,
                                ChapterId = selectedChapter.ChapterId,
                                Title = selectedChapter.Title,
                                Goal = selectedChapter.Goal,
                                Content = selectedChapter.Content,
                                Order = selectedChapter.Order
                            };
                        }
                    }
                    else
                    {
                        ChapterForm = new ChapterFormInput
                        {
                            TopicId = selectedTopic.TopicID,
                            Order = 1
                        };
                    }
                }
            }
            else
            {
                TopicForm = new TopicFormInput();
                ChapterForm = new ChapterFormInput();
            }
        }

        private class ChapterPayload
        {
            public string Title { get; set; } = "";
            public string Goal { get; set; } = "";
            public string Content { get; set; } = "";
            public int Order { get; set; } = 1;
        }
    }
}
