using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class Appointment
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public string Description { get; set; }
        public bool IsLeaveShift { get; set; }

        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public Guid StaffMemberId { get; set; }
        public virtual StaffMember StaffMember { get; set; }

        public int VersionCreatedId { get; set; }
        public virtual CalendarVersion VersionCreated { get; set; }

        public int? VersionCancelledId { get; set; }
        public virtual CalendarVersion VersionCancelled { get; set; }
    }
}
