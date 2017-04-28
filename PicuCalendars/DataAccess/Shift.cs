using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class Shift
    {
        public Guid DepartmentId { get; set; }
        public string Code { get; set; }

        public string Description { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public int DurationMins { get; set; }
        public bool LeaveShift { get; set; }

        [NotMapped]
        public TimeSpan Duration
        {
            get { return new TimeSpan(0, DurationMins, 0); }
            set { DurationMins = (int)value.TotalMinutes; }
        }

        public virtual Department Department { get; set; }
    }
}
