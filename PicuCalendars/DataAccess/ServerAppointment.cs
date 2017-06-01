using System;
using System.ComponentModel.DataAnnotations;

namespace PicuCalendars.DataAccess
{
    public class ServerAppointment
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }

        [StringLength(Shift.DescriptionLength)]
        public string Description { get; set; }
        public bool IsLeaveShift { get; set; }

        public Guid RosterId { get; set; }
        public virtual ServerRoster Roster { get; set; }
        [StringLength(StaffMember.StaffMemberCodeLength, MinimumLength =StaffMember.StaffMemberCodeMinLength)]
        public string StaffMemberCode { get; set; }
        public virtual ServerStaffMember Staff { get; set; }

        public int VersionCreatedId { get; set; }
        public virtual CalendarVersion VersionCreated { get; set; }

        public int? VersionCancelledId { get; set; }
        public virtual CalendarVersion VersionCancelled { get; set; }
    }
}
