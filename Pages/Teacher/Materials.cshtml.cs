using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mathly.Data;
using Mathly.Models;

namespace Mathly.Pages.Teacher
{
    [Authorize(Roles = "teacher")]
    public class MaterialsModel : PageModel
    {
        private readonly MathlyDbContext _db;
        private readonly IWebHostEnvironment _env;

        private string TeacherID => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public MaterialsModel(MathlyDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public string TeacherName { get; set; } = "Teacher";
        public List<MaterialItemDto> MaterialsList { get; set; } = new();
        public List<SelectListItem> TopicOptions { get; set; } = new();

        [BindProperty]
        public string SelectedTopicID { get; set; }

        [BindProperty]
        public string CustomTitle { get; set; }

        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class MaterialItemDto
        {
            public string MaterialID { get; set; }
            public string TopicName { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string FileType { get; set; }
        }

        private class TopicDto
        {
            public string TopicID { get; set; }
            public string TopicName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageDataAsync();
            return Page();
        }

        private async Task LoadPageDataAsync()
        {
            var teacher = await _db.Teachers.FindAsync(TeacherID);
            if (teacher != null)
            {
                TeacherName = teacher.TeacherName;
            }

            // Fetch topics for this teacher
            var topicDtos = await _db.Database
                .SqlQueryRaw<TopicDto>(
                    "SELECT topicID AS TopicID, topicName AS TopicName FROM topic WHERE userID = {0}", TeacherID)
                .ToListAsync();

            TopicOptions = topicDtos.Select(t => new SelectListItem
            {
                Value = t.TopicID,
                Text = t.TopicName
            }).ToList();

            // Fetch all study materials
            var allMaterials = await _db.StudyMaterials
                .Include(m => m.Topic)
                .ToListAsync();

            MaterialsList = allMaterials.Select(m =>
            {
                string fnStr = m.FileName != null ? Encoding.UTF8.GetString(m.FileName) : "document.pdf";
                return new MaterialItemDto
                {
                    MaterialID = m.MaterialID,
                    TopicName = m.Topic != null ? m.Topic.TopicName : "General",
                    FileName = fnStr,
                    FilePath = $"/uploads/materials/{fnStr}",
                    FileType = fnStr.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : "DOC"
                };
            }).ToList();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (string.IsNullOrEmpty(SelectedTopicID))
            {
                ErrorMessage = "Please select a topic to attach the material.";
                await LoadPageDataAsync();
                return Page();
            }

            if (UploadFile == null || UploadFile.Length == 0)
            {
                ErrorMessage = "Please select a valid PDF or DOC file to upload.";
                await LoadPageDataAsync();
                return Page();
            }

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "materials");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string safeFileName = Path.GetFileName(UploadFile.FileName);
                if (!string.IsNullOrEmpty(CustomTitle))
                {
                    string ext = Path.GetExtension(UploadFile.FileName);
                    safeFileName = $"{CustomTitle.Trim().Replace(" ", "_")}{ext}";
                }

                string filePath = Path.Combine(uploadsFolder, safeFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadFile.CopyToAsync(stream);
                }

                string newMaterialID = "mat_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var newMaterial = new StudyMaterial
                {
                    MaterialID = newMaterialID,
                    TopicID = SelectedTopicID,
                    FileName = Encoding.UTF8.GetBytes(safeFileName)
                };

                _db.StudyMaterials.Add(newMaterial);

                // Send notification to all students about the new material
                var topic = await _db.Topics.FindAsync(SelectedTopicID);
                string topicName = topic != null ? topic.TopicName : "Math";
                var students = await _db.Students.ToListAsync();

                foreach (var std in students)
                {
                    string notifID = "notif_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    var matNotif = new Notification
                    {
                        NotificationID = notifID,
                        UserID = std.UserID,
                        Message = $"📄 New study material '{safeFileName}' added for {topicName}!",
                        Type = "material",
                        IsRead = false
                    };
                    _db.Notifications.Add(matNotif);
                }

                await _db.SaveChangesAsync();

                SuccessMessage = $"Successfully uploaded '{safeFileName}'!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to upload file: {ex.Message}";
            }

            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string materialId)
        {
            if (!string.IsNullOrEmpty(materialId))
            {
                var mat = await _db.StudyMaterials.FindAsync(materialId);
                if (mat != null)
                {
                    _db.StudyMaterials.Remove(mat);
                    await _db.SaveChangesAsync();
                    SuccessMessage = "Study material removed successfully.";
                }
            }

            await LoadPageDataAsync();
            return Page();
        }
    }
}
