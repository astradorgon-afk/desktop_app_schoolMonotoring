using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

public class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<StudentQr> StudentQrCodes => Set<StudentQr>();

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

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProfileImageUrl).HasColumnName("profile_image_url");
            entity.Property(e => e.Lrn).HasColumnName("lrn").IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(e => e.MiddleName).HasColumnName("middle_name");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.GuardianName).HasColumnName("guardian_name");
            entity.Property(e => e.GuardianContact).HasColumnName("guardian_contact");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.StudentNumber).HasColumnName("student_number").IsRequired();
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.ContactNo).HasColumnName("contact_no");
            entity.Property(e => e.PreviousSchool).HasColumnName("previous_school");
            entity.Property(e => e.PreferredGradeLevelId).HasColumnName("preferred_grade_level_id");
            entity.Property(e => e.PreferredCurriculumId).HasColumnName("preferred_curriculum_id");
            entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");

            entity.HasIndex(e => e.Lrn).HasDatabaseName("idx_students_lrn").IsUnique();
            entity.HasIndex(e => e.StudentNumber).HasDatabaseName("idx_students_student_number").IsUnique();
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_students_user_id").IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<StudentQr>(entity =>
        {
            entity.ToTable("student_qr_tbl");

            entity.HasKey(e => e.IdstudentQr);

            entity.Property(e => e.IdstudentQr).HasColumnName("idstudent_qr");
            entity.Property(e => e.StudentId).HasColumnName("student_id").IsRequired();
            entity.Property(e => e.QrContent).HasColumnName("qr_content").IsRequired();
            entity.Property(e => e.QrCode).HasColumnName("qr_code").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.StudentId).HasDatabaseName("idx_student_qr_student_id").IsUnique();

            entity.HasOne(e => e.Student)
                .WithOne(s => s.QrCode)
                .HasForeignKey<StudentQr>(e => e.StudentId);
        });
    }
}
