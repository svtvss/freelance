using Microsoft.EntityFrameworkCore;

namespace freelance.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<NotificationRules> NotificationRules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserProject> UsersProjects { get; set; }
    }
}
