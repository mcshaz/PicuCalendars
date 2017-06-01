using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;

namespace PicuCalendars.DataAccess
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<CalendarContext>());
        }

        public DbSet<ServerAppointment> Appointments { get; set; }
        public DbSet<CalendarVersion> Versions { get; set; }
        public DbSet<ServerRoster> Rosters { get; set; }
        public DbSet<ServerShift> Shifts { get; set; }
        public DbSet<ServerStaffMember> Staff { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            const string schema = "Calendar";
            modelBuilder.Entity<ServerAppointment>()
                .ToTable("Appointments",schema)
                .HasKey(e => e.Id);

            modelBuilder.Entity<CalendarVersion>()
                .ToTable("Versions", schema)
                .HasKey(e => e.Number);

            modelBuilder.Entity<ServerRoster>()
                .ToTable("Rosters", schema)
                .HasKey(e => e.Id);

            modelBuilder.Entity<ServerStaffMember>()
                .ToTable("StaffMembers", schema)
                .HasKey(e => new { e.RosterId, e.StaffMemberCode });

            modelBuilder.Entity<ServerShift>()
                .ToTable("Shifts", schema)
                .HasKey(e => new { e.RosterId, e.Code });

            modelBuilder.Entity<ServerAppointment>()
                .HasRequired(e => e.Staff)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e=>new { e.RosterId, e.StaffMemberCode })
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

            //.Property(e => e.Abbreviation)
            //.HasColumnAnnotation("UniqueAbbreviation", new IndexAnnotation(new IndexAttribute { IsUnique = true }));

            modelBuilder.Entity<ServerRoster>()
                .Property(e => e.Secret)
                .IsFixedLength()
                .HasMaxLength(64);

            modelBuilder.Entity<ServerShift>()
                .HasRequired(e => e.Roster)
                .WithMany()
                .HasForeignKey(e=>e.RosterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerStaffMember>()
                .HasRequired(e => e.Roster)
                .WithMany(e => e.Staff)
                .HasForeignKey(e=>e.RosterId)
                .WillCascadeOnDelete(false);

            /*
            const string compositeStaffDptAbbrev = "IX_StaffDepartmentAbbrev";
            modelBuilder.Entity<ServerStaffMember>().Property(e => e.RosterId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 1) { IsUnique = true }));
            */
                //.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(compositeStaffDptAbbrev, 2) { IsUnique = true }));

            modelBuilder.Entity<ServerStaffMember>()
                .HasOptional(e => e.LastViewedVersion)
                .WithMany()
                .HasForeignKey(e=>e.LastViewedVersionId);

        }
    }
}
