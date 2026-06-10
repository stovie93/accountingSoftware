using ConserviceAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConserviceAccounting.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("user_id");

        builder.Property(u => u.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.UserEmail)
            .HasColumnName("user_email")
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(u => u.UserEmail).IsUnique();

        builder.Property(u => u.UserPasswordHash)
            .HasColumnName("user_password_hash")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.UserFirstName)
            .HasColumnName("user_first_name")
            .HasMaxLength(100);

        builder.Property(u => u.UserLastName)
            .HasColumnName("user_last_name")
            .HasMaxLength(100);

        builder.Property(u => u.UserTitle)
            .HasColumnName("user_title")
            .HasMaxLength(100);

        builder.Property(u => u.UserGroup)
            .HasColumnName("user_group")
            .HasMaxLength(100);

        builder.Property(u => u.UserPhone)
            .HasColumnName("user_phone")
            .HasMaxLength(50);

        builder.Property(u => u.UserDepartment)
            .HasColumnName("user_department")
            .HasMaxLength(100);

        builder.Property(u => u.UserRole)
            .HasColumnName("user_role")
            .HasConversion<int>();

        builder.Property(u => u.UserIsActive)
            .HasColumnName("user_is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.UserCreatedAt)
            .HasColumnName("user_created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UserUpdatedAt)
            .HasColumnName("user_updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UserLastLoginAt)
            .HasColumnName("user_last_login_at");
    }
}
