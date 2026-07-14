using KarateSchool.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace KarateSchool.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<KarateClass> KarateClasses => Set<KarateClass>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<BeltPromotion> BeltPromotions => Set<BeltPromotion>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- User hierarchy: table-per-type (TPT). Each subclass table shares the Users PK. ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Phone).IsRequired().HasMaxLength(30);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.Property(s => s.BeltRank).IsRequired().HasMaxLength(50);
            entity.Property(s => s.EmergencyContact).HasMaxLength(200);
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable("Instructors");
            entity.Property(i => i.Specialty).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Certification).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.ToTable("Administrators");
            entity.Property(a => a.AccessLevel).IsRequired().HasMaxLength(50);
        });

        // --- KarateClass: taught by exactly one Instructor; room/instructor cannot double-book a schedule slot. ---
        modelBuilder.Entity<KarateClass>(entity =>
        {
            entity.ToTable("KarateClasses");
            entity.HasKey(c => c.ClassId);
            entity.Property(c => c.ClassName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Schedule).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Room).IsRequired().HasMaxLength(50);

            entity.HasOne(c => c.Instructor)
                .WithMany(i => i.ClassesTaught)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.Room, c.Schedule }).IsUnique();
            entity.HasIndex(c => new { c.InstructorId, c.Schedule }).IsUnique();
        });

        // --- Enrollment: junction table resolving Student <-> KarateClass (M:M). ---
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(e => e.EnrollmentId);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.KarateClass)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
        });

        // --- Attendance: tracks a Student's presence in a KarateClass session. ---
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances");
            entity.HasKey(a => a.AttendanceId);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(a => a.Student)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.KarateClass)
                .WithMany(c => c.AttendanceRecords)
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- BeltPromotion: Instructor recommends, Student earns, Administrator approves. ---
        modelBuilder.Entity<BeltPromotion>(entity =>
        {
            entity.ToTable("BeltPromotions");
            entity.HasKey(p => p.PromotionId);
            entity.Property(p => p.NewBelt).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Notes).IsRequired();

            entity.HasOne(p => p.Student)
                .WithMany(s => s.BeltPromotions)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Instructor)
                .WithMany(i => i.BeltPromotionsRecommended)
                .HasForeignKey(p => p.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Payment: a Student makes payments toward tuition/fees. ---
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.PaymentId);
            entity.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Method).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Announcement: an Administrator posts announcements. ---
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcements");
            entity.HasKey(a => a.AnnouncementId);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Content).IsRequired();

            entity.HasOne(a => a.Administrator)
                .WithMany(ad => ad.Announcements)
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
