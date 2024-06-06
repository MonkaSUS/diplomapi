using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Sockets;

namespace ThreeMorons.Model;

public partial class ThreeMoronsContext : DbContext
{
    public ThreeMoronsContext()
    {
    }

    public ThreeMoronsContext(DbContextOptions<ThreeMoronsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Period> Periods { get; set; }

    public virtual DbSet<SkippedClass> SkippedClasses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentDelay> StudentDelays { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Session> Sessions { get; set; }
    public virtual DbSet<UserClass> UserClasses { get; set; }
    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<DbServiceUser> DbServiceUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#pragma warning disable CS1030 // Директива #warning
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("workstation id = ThreeMorons.mssql.somee.com; packet size = 4096; user id = TigerLion46_SQLLogin_1; pwd=1ymuz53ub9;data source = ThreeMorons.mssql.somee.com; persist security info=False;initial catalog = ThreeMorons; TrustServerCertificate=True");
#pragma warning restore CS1030 // Директива #warning

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(e =>
        {
            e.HasKey(en => en.Id);
            e.Property(en => en.Title).HasMaxLength(100);
            e.Property(en => en.Body).HasMaxLength(500);
            e.Property(en => en.Author).HasMaxLength(100);
            e.Property(en => en.TargetAudience).HasMaxLength(30);
        });
        modelBuilder.Entity<DbServiceUser>(e =>
        {
            e.ToTable("DbServiceUser");
            e.HasKey(p => p.id);
            e.Property(p => p.user_login).HasMaxLength(50);
            e.Property(p => p.user_password).HasMaxLength(50);
            e.Property(p => p.telegram_id).HasMaxLength(50);
            e.Property(p => p.db_type).HasMaxLength(50);
        });
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupName);

            entity.ToTable("Group");

            entity.Property(e => e.GroupName).HasMaxLength(10);

            entity.HasOne(d => d.GroupCuratorNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.GroupCurator)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Group_User");
        });
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.ToTable("Session");
            entity.Property(e => e.JwtToken).HasMaxLength(500);
            entity.Property(e => e.RefreshToken).HasMaxLength(300);
        });
        modelBuilder.Entity<Period>(entity =>
        {
            entity.ToTable("Period");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EndTime).HasPrecision(2);
            entity.Property(e => e.StartTime).HasPrecision(2);
        });

        modelBuilder.Entity<SkippedClass>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.StudNumber).HasMaxLength(5);

            entity.HasOne(d => d.StudNumberNavigation).WithMany(p => p.SkippedClasses)
                .HasForeignKey(d => d.StudNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SkippedClasses_Student");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudNumber);

            entity.ToTable("Student");

            entity.Property(e => e.StudNumber).HasMaxLength(5);
            entity.Property(e => e.GroupName).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(40);
            entity.Property(e => e.Patronymic).HasMaxLength(40);
            entity.Property(e => e.PhoneNumber).HasMaxLength(11);
            entity.Property(e => e.Surname).HasMaxLength(40);

            entity.HasOne(d => d.GroupNameNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.GroupName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Student_Group");
        });

        modelBuilder.Entity<StudentDelay>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ClassName).HasMaxLength(20);
            entity.Property(e => e.Delay).HasPrecision(2);
            entity.Property(e => e.StudNumber).HasMaxLength(5);

            entity.HasOne(d => d.StudNumberNavigation).WithMany(p => p.StudentDelays)
                .HasForeignKey(d => d.StudNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentDelays_Student").IsRequired(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Login).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(40);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Patronymic).HasMaxLength(40);
            entity.Property(e => e.Surname).HasMaxLength(40);
            entity.Property(e => e.Salt).HasMaxLength(20);
            entity.HasOne(d => d.UserClass).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserClass");
            entity.HasOne(e => e.Student).WithOne(p => p.User)
            .HasForeignKey<User>(x => x.StudNumber)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_User_Student");
        });

        modelBuilder.Entity<UserClass>(entity =>
        {
            entity.ToTable("UserClass");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
