using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class CalendarVersion
    {
        public int Number { get; set; }
        public DateTime Created { get; set; }

        public virtual ICollection<Appointment> CreatedAppointments { get; set; }
        public virtual ICollection<Appointment> CancelledAppointments { get; set; }
    }
}
