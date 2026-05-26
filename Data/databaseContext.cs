using Microsoft.EntityFrameworkCore;
using Mathly.Models;

namespace Mathly.Data
{
    public class MathlyDbContext : DbContext
    {
        public MathlyDbContext(DbContextOptions<MathlyDbContext> options) : base(options) { }

        public DbSet<LoginCredentials> LoginCredentials { get; set; }
        public DbSet<StudentInfo> Students { get; set; }
        public DbSet<TeacherInfo> Teachers { get; set; }
        public DbSet<AdminInfo> Admins { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<StudentTopic> StudentTopics { get; set; }
        public DbSet<Quizzes> Quizzes { get; set; }
        public DbSet<QuizQuestions> QuizQuestions { get; set; }
        public DbSet<QuizStudentAttempt> QuizStudentAttempts { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<StudyMaterial> StudyMaterials { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<LearningProgress> LearningProgress { get; set; }
        public DbSet<Badges> Badges { get; set; }
        public DbSet<StudentBadges> StudentBadges { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}