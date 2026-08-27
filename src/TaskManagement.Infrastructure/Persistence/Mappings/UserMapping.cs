using TaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskManagement.Infrastructure.Persistence.Mappings;

class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasColumnType("varchar(100)");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasColumnType("varchar(256)");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("varchar(500)");

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.ToTable("Users");
    }
}
