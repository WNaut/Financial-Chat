using FinancialChat.Persistence.Configurations;
using FinancialChat.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialChat.Persistence
{
    public class FinancialChatDbContext : DbContext
    {
        public FinancialChatDbContext(DbContextOptions<FinancialChatDbContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
