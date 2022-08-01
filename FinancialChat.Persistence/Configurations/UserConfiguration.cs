using FinancialChat.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialChat.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(u => u.Password)
                .HasMaxLength(800)
                .IsRequired();

            builder.Property(u => u.CreationDate)
                .IsRequired();

            builder.Property(u => u.Status)
                .IsRequired();
        }
    }
}
