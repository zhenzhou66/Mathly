using Microsoft.EntityFrameworkCore;
using testWebApp.Models;

namespace testWebApp.Data   
{
    public class testDbContext : DbContext
    {
        public testDbContext(DbContextOptions<testDbContext> options) : base(options) { }

        public DbSet<StudentInfo> Students { get; set; }

        // add the rest of your tables here
    }
}