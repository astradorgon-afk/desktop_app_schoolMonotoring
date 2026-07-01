using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

public class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentQr> StudentQrCodes => Set<StudentQr>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassOffering> ClassOfferings => Set<ClassOffering>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();
public DbSet<Enrollment> Enrollments => Set<Enrollment>();
public DbSet<Room> Rooms => Set<Room>();
public DbSet<SchoolYear> SchoolYears => Set<SchoolYear>();
public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>().IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().IsRequired();
            entity.Property(e => e.CanLogin).HasColumnName("can_login").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.Username).HasDatabaseName("idx_users_username");
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
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
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
            entity.HasOne(e => e.Student).WithOne(s => s.QrCode).HasForeignKey<StudentQr>(e => e.StudentId);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("teachers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProfileImageUrl).HasColumnName("profile_image_url");
            entity.Property(e => e.EmployeeNo).HasColumnName("employee_no");
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(e => e.MiddleName).HasColumnName("middle_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.ContactNo).HasColumnName("contact_no");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Specialization).HasColumnName("specialization");
            entity.Property(e => e.AdvisoryAssignmentStatus).HasColumnName("advisory_assignment_status");
            entity.Property(e => e.EmploymentStatus).HasColumnName("employment_status");
            entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");
            entity.HasIndex(e => e.EmployeeNo).HasDatabaseName("idx_teachers_employee_no").IsUnique();
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_teachers_user_id").IsUnique();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.ToTable("teacher_subject_tbl");
            entity.HasKey(e => e.IdteacherSubject);
            entity.Property(e => e.IdteacherSubject).HasColumnName("idteacher_subject");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id").IsRequired();
            entity.Property(e => e.SubjectTitle).HasColumnName("subject_title").HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasOne(e => e.Teacher).WithMany().HasForeignKey(e => e.TeacherId);
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.ToTable("sections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.SchoolYearId).HasColumnName("school_year_id");
            entity.Property(e => e.GradeLevelId).HasColumnName("grade_level_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.AdviserTeacherId).HasColumnName("adviser_teacher_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsArchived).HasColumnName("is_archived");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Code).HasColumnName("code").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GradeLevelId).HasColumnName("grade_level_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ClassOffering>(entity =>
        {
            entity.ToTable("class_offerings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.SchoolYearId).HasColumnName("school_year_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.CurriculumId).HasColumnName("curriculum_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.Room).HasColumnName("room");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.Teacher).WithMany(t => t.ClassOfferings).HasForeignKey(e => e.TeacherId);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId);
            entity.HasMany(e => e.Schedules).WithOne(s => s.ClassOffering).HasForeignKey(s => s.ClassOfferingId);
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.ToTable("class_schedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ClassOfferingId).HasColumnName("class_offering_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.TimeSlotId).HasColumnName("time_slot_id");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
        modelBuilder.Entity<GradeLevel>(entity =>
        {
            entity.ToTable("grade_levels");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Code).HasColumnName("code").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.SchoolYearId).HasColumnName("school_year_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.GradeLevelId).HasColumnName("grade_level_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CurriculumId).HasColumnName("curriculum_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.EnrolledAt).HasColumnName("enrolled_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Code).HasColumnName("code").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<SchoolYear>(entity =>
        {
            entity.ToTable("school_years");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.ToTable("attendance_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ClassOfferingId).HasColumnName("class_offering_id");
            entity.Property(e => e.SessionDate).HasColumnName("session_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.ClassOffering).WithMany().HasForeignKey(e => e.ClassOfferingId);
            entity.HasMany(e => e.Records).WithOne(r => r.Session).HasForeignKey(r => r.AttendanceSessionId);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("attendance_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.AttendanceSessionId).HasColumnName("attendance_session_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.MarkedByUserId).HasColumnName("marked_by_user_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId);
        });
    }
}
