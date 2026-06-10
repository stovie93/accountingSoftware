using PropertyViewerAccounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PropertyViewerAccounting.Infrastructure.Data.Configurations;

public class ClientAccountingConnectionConfiguration : IEntityTypeConfiguration<ClientAccountingConnection>
{
    public void Configure(EntityTypeBuilder<ClientAccountingConnection> builder)
    {
        builder.ToTable("client_accounting_connections");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(c => c.AccountingSoftwareId)
            .HasColumnName("accounting_software_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // Database connection fields
        builder.Property(c => c.ConnectionString)
            .HasColumnName("connection_string")
            .HasMaxLength(1000);

        builder.Property(c => c.DatabaseName)
            .HasColumnName("database_name")
            .HasMaxLength(200);

        builder.Property(c => c.DatabaseServer)
            .HasColumnName("database_server")
            .HasMaxLength(200);

        builder.Property(c => c.DatabaseUsername)
            .HasColumnName("database_username")
            .HasMaxLength(200);

        builder.Property(c => c.DatabasePassword)
            .HasColumnName("database_password")
            .HasMaxLength(500);

        // API connection fields
        builder.Property(c => c.ApiEndpoint)
            .HasColumnName("api_endpoint")
            .HasMaxLength(500);

        builder.Property(c => c.ApiKey)
            .HasColumnName("api_key")
            .HasMaxLength(500);

        builder.Property(c => c.ApiSecret)
            .HasColumnName("api_secret")
            .HasMaxLength(500);

        // SFTP connection fields
        builder.Property(c => c.SftpHost)
            .HasColumnName("sftp_host")
            .HasMaxLength(200);

        builder.Property(c => c.SftpPort)
            .HasColumnName("sftp_port");

        builder.Property(c => c.SftpUsername)
            .HasColumnName("sftp_username")
            .HasMaxLength(200);

        builder.Property(c => c.SftpPassword)
            .HasColumnName("sftp_password")
            .HasMaxLength(500);

        builder.Property(c => c.SftpPath)
            .HasColumnName("sftp_path")
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.LastTestedAt)
            .HasColumnName("last_tested_at");

        builder.Property(c => c.LastTestStatus)
            .HasColumnName("last_test_status")
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AccountingSoftware)
            .WithMany(a => a.Connections)
            .HasForeignKey(c => c.AccountingSoftwareId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.AccountingSoftwareId);
    }
}
