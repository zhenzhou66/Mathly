using Microsoft.EntityFrameworkCore;
using Mathly.Models;

namespace Mathly.Data
{
    public class MathlyDbContext : DbContext
    {
        public MathlyDbContext(DbContextOptions<MathlyDbContext> options) : base(options) { }

        public DbSet<StudentInfo> Students { get; set; }

        // add the rest of your tables here
    }
}