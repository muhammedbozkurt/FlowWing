using FlowWing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlowWing.DataAccess
{
    public class FlowWingDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public FlowWingDbContext(DbContextOptions<FlowWingDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DatabaseConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<ScheduledEmail> ScheduledEmails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
