using AttendanceUI.Data.Entities;
using AttendanceUI.Services;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

/// <summary>
/// Populates a freshly-created (empty) database with a realistic demo dataset:
/// 200 teachers, 200 students, 200 rooms, 10 subjects, sections, enrollments,
/// and one class offering + schedule per teacher so that every teacher and every
/// room is occupied. Two deliberate schedule clashes are planted in each grade
/// level so the conflict detector and resolve UI have something to work on.
/// <see cref="SqliteActivitySeeder"/> then adds six weeks of attendance, leave requests,
/// excuse letters and audit entries so dashboards and reports have data.
/// Intended for the portable SQLite profile. No-ops if the database already
/// contains users.
/// </summary>
public static class SqliteSeeder
{
    private const int TeacherCount = 200;
    private const int StudentCount = 200;
    private const int RoomCount = 200;
    private const int SectionCount = 20;   // 5 per grade level

    // Demo credentials for the portable profile are deliberately trivial:
    //   admin   / admin
    //   teacher / teacher   (also teacher2 … teacher200, all with the password "teacher")
    //   student / student   (also student2 … student200, all with the password "student")
    // This database ships as a local demo — never point the portable profile at real records.
    private const string AdminUsername = "admin";
    private const string TeacherPassword = "teacher";
    private const string StudentPassword = "student";

    // PBKDF2 work factor for the demo accounts. These are throwaway credentials whose
    // passwords are already public, so the production default (100,000) buys nothing
    // here and only slows first run on a phone. Accounts created through the app are
    // unaffected and still use the full default.
    private const int DemoIterationCount = 1_000;

    /// <summary>The first account of a role is plain "teacher" / "student"; the rest are numbered from 2.</summary>
    private static string DemoUsername(string role, int index) => index == 0 ? role : $"{role}{index + 1}";

    private static readonly string[] FirstNames =
    [
        "Adrian", "Althea", "Benjamin", "Bianca", "Christian", "Clarisse", "Dominic", "Daphne",
        "Elijah", "Erika", "Francis", "Frances", "Gian", "Gwen", "Harold", "Hannah",
        "Ivan", "Ingrid", "Jerome", "Janine", "Kenneth", "Katrina", "Lorenzo", "Lorraine",
        "Matteo", "Mikaela", "Noel", "Nadine", "Owen", "Pauline", "Patrick", "Rhea",
        "Sebastian", "Samantha", "Tristan", "Trisha", "Vincent", "Vanessa", "Xander", "Ysabel",
    ];

    private static readonly string[] LastNames =
    [
        "Abad", "Agustin", "Alonzo", "Arellano", "Balagtas", "Belmonte", "Buenaventura", "Cabrera",
        "Calderon", "Carpio", "Concepcion", "Corpuz", "Dantes", "De Guzman", "De Leon", "Dizon",
        "Enriquez", "Escobar", "Espiritu", "Estrada", "Evangelista", "Francisco", "Galang", "Gatchalian",
        "Guevarra", "Hernandez", "Ignacio", "Jimenez", "Lacson", "Legaspi", "Macaraeg", "Magbanua",
        "Natividad", "Nepomuceno", "Padilla", "Panganiban", "Quiambao", "Robles", "Sarmiento", "Tolentino",
        "Umali", "Valenzuela", "Velasco", "Vergara", "Zamora",
    ];

    private static readonly string[] Streets =
    [
        "Acacia St.", "Bayanihan Rd.", "Camia St.", "Gumamela St.", "Ilang-Ilang St.",
        "Kalachuchi Ave.", "Molave Rd.", "Mahogany Ave.", "Kamagong St.", "Santan St.",
    ];

    private static readonly (string Code, string Title)[] Subjects =
    [
        ("MATH", "Mathematics"),
        ("ENG", "English"),
        ("SCI", "Science"),
        ("FIL", "Filipino"),
        ("AP", "Araling Panlipunan"),
        ("MAPEH", "MAPEH"),
        ("TLE", "Technology and Livelihood Education"),
        ("ESP", "Edukasyon sa Pagpapakatao"),
        ("COMP", "Computer Science"),
        ("RES", "Research"),
    ];

    private static readonly string[] SectionNames =
        ["Sampaguita", "Rizal", "Mabini", "Bonifacio", "Narra"];

    private static readonly (TimeOnly Start, TimeOnly End)[] Slots =
    [
        (new(7, 30), new(8, 30)),
        (new(8, 30), new(9, 30)),
        (new(9, 30), new(10, 30)),
        (new(10, 30), new(11, 30)),
        (new(13, 0), new(14, 0)),
        (new(14, 0), new(15, 0)),
        (new(15, 0), new(16, 0)),
        (new(16, 0), new(17, 0)),
    ];

    public static void Seed(SchoolDbContext context)
    {
        // The school itself is only seeded into an empty database.
        if (!context.Users.Any())
            SeedSchool(context);

        // Earlier demo files enrolled students as "ENROLLED", which most screens (class rosters,
        // QR scanning, the student portal) do not count as enrolled. The app itself never
        // writes that value, so only seeded rows are touched.
        context.Enrollments.Where(e => e.Status == "ENROLLED")
            .ExecuteUpdate(u => u.SetProperty(e => e.Status, "ACTIVE"));

        // Activity is separate so a demo file created before it existed is topped up too.
        if (!context.AttendanceSessions.Any())
            SqliteActivitySeeder.Seed(context);
    }

    private static void SeedSchool(SchoolDbContext context)
    {
        var now = DateTime.UtcNow;
        var rng = new Random(20261112);

        // ── School year & grade levels (grade Ids must be 1..4 = G7..G10) ──
        var schoolYear = new SchoolYear
        {
            Name = "2025-2026",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2026, 3, 31),
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.SchoolYears.Add(schoolYear);

        var gradeLevels = new List<GradeLevel>();
        for (var g = 7; g <= 10; g++)
        {
            gradeLevels.Add(new GradeLevel
            {
                Code = $"G{g}",
                Name = $"Grade {g}",
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.GradeLevels.AddRange(gradeLevels);

        // ── Subjects ──
        var subjects = Subjects.Select(s => new Subject
        {
            Code = s.Code,
            Title = s.Title,
            Description = $"{s.Title} subject",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        }).ToList();
        context.Subjects.AddRange(subjects);

        // ── Rooms ──
        var rooms = new List<Room>();
        for (var i = 1; i <= RoomCount; i++)
        {
            rooms.Add(new Room
            {
                Code = $"R{i:D3}",
                Name = $"Room {i}",
                Capacity = 30 + rng.Next(0, 4) * 5, // 30/35/40/45
                IsActive = true,
            });
        }
        context.Rooms.AddRange(rooms);

        // ── Users (1 admin + 200 teacher logins + 200 student logins) ──
        // Every account of a role shares that role's password, so one hash per role is enough.
        var teacherHash = PasswordHelper.Hash(TeacherPassword, DemoIterationCount);
        var studentHash = PasswordHelper.Hash(StudentPassword, DemoIterationCount);

        var admin = new User
        {
            Name = "System Administrator",
            Username = AdminUsername,
            PasswordHash = PasswordHelper.Hash(AdminUsername, DemoIterationCount),
            Role = UserRole.SUPERADMIN,
            Status = UserStatus.ACTIVE,
            CanLogin = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.Users.Add(admin);

        var teacherUsers = new List<User>();
        var teacherNames = new List<(string First, string Last)>();
        for (var i = 0; i < TeacherCount; i++)
        {
            var (first, last) = RandomName(rng);
            teacherNames.Add((first, last));
            teacherUsers.Add(new User
            {
                Name = $"{first} {last}",
                Username = DemoUsername("teacher", i),
                PasswordHash = teacherHash,
                Role = UserRole.TEACHER,
                Status = UserStatus.ACTIVE,
                CanLogin = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Users.AddRange(teacherUsers);

        var studentUsers = new List<User>();
        var studentNames = new List<(string First, string Last)>();
        for (var i = 0; i < StudentCount; i++)
        {
            var (first, last) = RandomName(rng);
            studentNames.Add((first, last));
            studentUsers.Add(new User
            {
                Name = $"{first} {last}",
                Username = DemoUsername("student", i),
                PasswordHash = studentHash,
                Role = UserRole.STUDENT,
                Status = UserStatus.ACTIVE,
                CanLogin = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Users.AddRange(studentUsers);

        context.SaveChanges(); // assigns Ids to years, grades, subjects, rooms, users

        // ── Teachers ──
        var teachers = new List<Teacher>();
        for (var i = 0; i < TeacherCount; i++)
        {
            var (first, last) = teacherNames[i];
            teachers.Add(new Teacher
            {
                UserId = teacherUsers[i].Id,
                EmployeeNo = $"EMP{i + 1:D4}",
                FirstName = first,
                LastName = last,
                Email = $"teacher{i + 1:D3}@school.edu.ph",
                ContactNo = RandomPhone(rng),
                HireDate = new DateTime(2015, 1, 1).AddDays(rng.Next(0, 3650)),
                Status = "ACTIVE",
                Specialization = Subjects[i % Subjects.Length].Title,
                EmploymentStatus = rng.Next(0, 4) == 0 ? "Contractual" : "Regular",
                AdvisoryAssignmentStatus = i < SectionCount ? "Advisory" : "Non-Advisory",
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Teachers.AddRange(teachers);

        // ── Students ──
        var students = new List<Student>();
        for (var i = 0; i < StudentCount; i++)
        {
            var (first, last) = studentNames[i];
            var birthdate = new DateTime(2010, 1, 1).AddDays(rng.Next(0, 1460)); // ~12-16 yrs
            students.Add(new Student
            {
                UserId = studentUsers[i].Id,
                Lrn = $"{100000000000L + i:D12}",
                StudentNumber = $"2025-{i + 1:D4}",
                FirstName = first,
                LastName = last,
                Birthdate = birthdate,
                Age = AgeFrom(birthdate),
                Sex = rng.Next(0, 2) == 0 ? "Male" : "Female",
                Address = $"{rng.Next(1, 240)} {Streets[rng.Next(Streets.Length)]}, Barangay {rng.Next(1, 20)}",
                ContactNo = RandomPhone(rng),
                GuardianName = $"{RandomName(rng).First} {last}",
                GuardianContact = RandomPhone(rng),
                PreviousSchool = "Central Elementary School",
                Status = "ACTIVE",
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Students.AddRange(students);

        context.SaveChanges(); // assigns teacher & student Ids

        // ── Sections (5 per grade level) ──
        var sections = new List<Section>();
        for (var i = 0; i < SectionCount; i++)
        {
            var grade = gradeLevels[i / 5];
            sections.Add(new Section
            {
                SchoolYearId = schoolYear.Id,
                GradeLevelId = grade.Id,
                Name = $"{grade.Code}-{SectionNames[i % SectionNames.Length]}",
                Capacity = 40,
                AdviserTeacherId = teachers[i].Id,
                IsArchived = false,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Sections.AddRange(sections);
        context.SaveChanges(); // assigns section Ids

        // ── Enrollments (each student into one section, 10 per section) ──
        var enrollments = new List<Enrollment>();
        for (var i = 0; i < StudentCount; i++)
        {
            var section = sections[i % SectionCount];
            enrollments.Add(new Enrollment
            {
                SchoolYearId = schoolYear.Id,
                StudentId = students[i].Id,
                GradeLevelId = section.GradeLevelId,
                SectionId = section.Id,
                CurriculumId = 1,
                Status = "ACTIVE",
                EnrolledAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        context.Enrollments.AddRange(enrollments);

        // ── Class offerings: one per teacher, each occupying a distinct room ──
        var offerings = new List<ClassOffering>();
        for (var k = 0; k < TeacherCount; k++)
        {
            var section = sections[k % SectionCount];
            offerings.Add(new ClassOffering
            {
                SchoolYearId = schoolYear.Id,
                SectionId = section.Id,
                // Keyed on the period (k / SectionCount), not k: every offering of a
                // section shares k % 10, so keying on k gave each section the same
                // subject ten times over. By period, each section gets all ten.
                SubjectId = subjects[(k / SectionCount) % subjects.Count].Id,
                TeacherId = teachers[k].Id,
                CurriculumId = 1,
                Status = "ACTIVE",
                Room = rooms[k].Code, // unique room per offering
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        // ── Deliberate schedule clashes: two per grade level ──
        // The generated grid never overlaps, which leaves the schedule page's conflict
        // detector, its section-card warning badges and the resolve UI with nothing to
        // show. Plant two clashes in every grade, alternating the second kind so all
        // three detector branches (section / teacher / room) get exercised.
        //
        // Sections are laid out five per grade, and offerings 20 apart share a section,
        // so offerings `SectionCount * period + section` addresses the grid directly.
        var extraSchedules = new List<(int OfferingIndex, int Period)>();
        var sharedRooms = new Dictionary<int, int>();   // clash offering index -> room owner index

        for (var grade = 0; grade < gradeLevels.Count; grade++)
        {
            var firstSection = grade * 5;

            // (1) Section double-booked — the period-2 class gains a second weekly
            //     meeting that lands on top of the same section's period-1 class.
            extraSchedules.Add((SectionCount * 2 + firstSection, Period: 1));

            if (grade % 2 == 0)
            {
                // (2a) Teacher double-booked — two sections of this grade already sit in
                //      period 3, so hand the second one the first one's teacher. That
                //      leaves the displaced teacher with no class of their own, which is
                //      fine in a 200-teacher demo and mirrors a real rostering mistake.
                var owner = SectionCount * 3 + firstSection;
                offerings[owner + 1].TeacherId = offerings[owner].TeacherId;
            }
            else
            {
                // (2b) Room double-booked — same trick in period 4, sharing a room code.
                var owner = SectionCount * 4 + firstSection;
                offerings[owner + 1].Room = offerings[owner].Room;
                sharedRooms[owner + 1] = owner;
            }
        }

        context.ClassOfferings.AddRange(offerings);
        context.SaveChanges(); // assigns offering Ids

        // ── Schedules: one per offering, plus the seeded clashes ──
        var schedules = new List<ClassSchedule>();
        for (var k = 0; k < TeacherCount; k++)
        {
            // Period-within-section drives a distinct (day, slot) pair.
            var roomIndex = sharedRooms.GetValueOrDefault(k, k);
            schedules.Add(NewSchedule(offerings[k].Id, rooms[roomIndex].Id, k / SectionCount));
        }

        foreach (var (offeringIndex, period) in extraSchedules)
            schedules.Add(NewSchedule(offerings[offeringIndex].Id, rooms[offeringIndex].Id, period));

        context.ClassSchedules.AddRange(schedules);

        context.SaveChanges();

        ClassSchedule NewSchedule(long offeringId, long roomId, int period)
        {
            var slot = Slots[period % Slots.Length];
            return new ClassSchedule
            {
                ClassOfferingId = offeringId,
                RoomId = roomId,
                DayOfWeek = (byte)((period % 5) + 1),   // Mon..Fri
                StartTime = slot.Start,
                EndTime = slot.End,
                CreatedAt = now,
                UpdatedAt = now,
            };
        }
    }

    private static (string First, string Last) RandomName(Random rng) =>
        (FirstNames[rng.Next(FirstNames.Length)], LastNames[rng.Next(LastNames.Length)]);

    private static string RandomPhone(Random rng) => $"09{rng.Next(100000000, 999999999)}";

    private static int AgeFrom(DateTime birthdate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthdate.Year;
        if (birthdate.Date > today.AddYears(-age)) age--;
        return age;
    }
}
