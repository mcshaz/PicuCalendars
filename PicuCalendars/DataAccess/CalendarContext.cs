using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;

namespace PicuCalendars.DataAccess
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer(new CreateCalendarContext());
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<CalendarVersion> Versions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<StaffMember> Staff { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>().HasKey(e => e.Id)
                .HasRequired(e => e.StaffMember)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e=>e.StaffMemberId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Appointment>()
                .HasRequired(e => e.Department)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e=>e.DepartmentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Appointment>()
                .HasRequired(e => e.VersionCreated)
                .WithMany(e=>e.CreatedAppointments)
                .HasForeignKey(e=>e.VersionCreatedId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Appointment>()
                .HasOptional(e => e.VersionCancelled)
                .WithMany(e=>e.CancelledAppointments)
                .HasForeignKey(e=>e.VersionCancelledId)
                .WillCascadeOnDelete(false);

            //.Property(e => e.Abbreviation)
            //.HasColumnAnnotation("UniqueAbbreviation", new IndexAnnotation(new IndexAttribute { IsUnique = true }));
            modelBuilder.Entity<CalendarVersion>()
                .HasKey(e => e.Number);

            modelBuilder.Entity<Department>().HasKey(e => e.Id)
                .Property(e=>e.Name)
                .HasMaxLength(64);

            modelBuilder.Entity<Shift>().HasKey(e => new { e.DepartmentId, e.Code });

            modelBuilder.Entity<Shift>().Property(e => e.Code)
                .IsFixedLength()
                .HasMaxLength(2);

            modelBuilder.Entity<Shift>().Property(e => e.Description)
                .HasMaxLength(128);

            modelBuilder.Entity<Shift>()
                .HasRequired(e => e.Department)
                .WithMany()
                .HasForeignKey(e=>e.DepartmentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StaffMember>().HasKey(e => e.Id)
                .HasRequired(e => e.Department)
                .WithMany(e => e.Staff)
                .HasForeignKey(e=>e.DepartmentId)
                .WillCascadeOnDelete(false);

            const string compositeStaffDptAbbrev = "IX_StaffDepartmentAbbrev";
            modelBuilder.Entity<StaffMember>().Property(e => e.DepartmentId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 1) { IsUnique = true }));

            modelBuilder.Entity<StaffMember>().Property(e => e.Abbreviation)
                .HasMaxLength(2)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 2) { IsUnique = true }));

            modelBuilder.Entity<StaffMember>()
                .HasOptional(e => e.LastViewedVersion)
                .WithMany()
                .HasForeignKey(e=>e.LastViewedVersionId);

            modelBuilder.Entity<StaffMember>()
                .Property(e => e.FullName)
                .HasMaxLength(256);
        }
    }
}
