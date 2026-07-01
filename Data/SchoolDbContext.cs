using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

public class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Id");

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            entity.Property(e => e.CanLogin)
                .HasColumnName("can_login")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.Username)
                .HasDatabaseName("idx_users_username");
        });
    }
}
