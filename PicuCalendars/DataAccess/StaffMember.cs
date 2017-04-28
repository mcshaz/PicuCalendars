using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class StaffMember
    {
        //the Guid is almost a securty token for downloading calendar - could use departmentId and Abbreviation otherwise
        public Guid Id { get; set; }
        public string FullName { get; set; }
        [StringLength(2, MinimumLength = 1)]
        public string Abbreviation { get; set; }

        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public int? LastViewedVersionId { get; set; }
        public virtual CalendarVersion LastViewedVersion { get; set; }
    }
}
