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

        public DbSet<ServerAppointment> Appointments { get; set; }
        public DbSet<CalendarVersion> Versions { get; set; }
        public DbSet<ServerRoster> Rosters { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ServerStaffMember> Staff { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerAppointment>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ServerAppointment>()
                .HasRequired(e => e.StaffMember)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e=>e.StaffInitials)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerAppointment>()
                .HasRequired(e => e.Roster)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e=>e.RosterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerAppointment>()
                .HasRequired(e => e.VersionCreated)
                .WithMany(e=>e.CreatedAppointments)
                .HasForeignKey(e=>e.VersionCreatedId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerAppointment>()
                .HasOptional(e => e.VersionCancelled)
                .WithMany(e=>e.CancelledAppointments)
                .HasForeignKey(e=>e.VersionCancelledId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerAppointment>()
                .Property(e => e.StaffInitials)
                .HasMaxLength(2);

            modelBuilder.Entity<CalendarVersion>()
                .HasKey(e => e.Number);

            //.Property(e => e.Abbreviation)
            //.HasColumnAnnotation("UniqueAbbreviation", new IndexAnnotation(new IndexAttribute { IsUnique = true }));

            modelBuilder.Entity<ServerRoster>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ServerRoster>()
                .Property(e=>e.DepartmentName)
                .HasMaxLength(64);

            modelBuilder.Entity<ServerRoster>()
                .Property(e => e.RosterName)
                .HasMaxLength(64);

            modelBuilder.Entity<ServerRoster>()
                .Property(e => e.Secret)
                .IsFixedLength()
                .HasMaxLength(64);

            modelBuilder.Entity<Shift>()
                .HasKey(e => new { e.RosterId, e.Code });

            modelBuilder.Entity<Shift>()
                .Property(e => e.Code)
                .IsFixedLength()
                .HasMaxLength(2);

            modelBuilder.Entity<Shift>()
                .Property(e => e.Description)
                .HasMaxLength(128);

            modelBuilder.Entity<Shift>()
                .HasRequired(e => e.Roster)
                .WithMany()
                .HasForeignKey(e=>e.RosterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerStaffMember>()
                .HasKey(e => e.Id); 

            modelBuilder.Entity<ServerStaffMember>()
                .HasRequired(e => e.Roster)
                .WithMany(e => e.Staff)
                .HasForeignKey(e=>e.RosterId)
                .WillCascadeOnDelete(false);


            const string compositeStaffDptAbbrev = "IX_StaffDepartmentAbbrev";
            modelBuilder.Entity<ServerStaffMember>().Property(e => e.RosterId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 1) { IsUnique = true }));

            modelBuilder.Entity<ServerStaffMember>()
                .Property(e => e.RosterCode)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 2) { IsUnique = true }));

            modelBuilder.Entity<ServerStaffMember>()
                .HasOptional(e => e.LastViewedVersion)
                .WithMany()
                .HasForeignKey(e=>e.LastViewedVersionId);

            modelBuilder.Entity<ServerStaffMember>()
                .Property(e => e.FullName)
                .HasMaxLength(256);
        }
    }
}
