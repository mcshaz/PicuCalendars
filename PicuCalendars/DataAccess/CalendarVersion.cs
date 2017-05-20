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

        public virtual ICollection<ServerAppointment> CreatedAppointments { get; set; }
        public virtual ICollection<ServerAppointment> CancelledAppointments { get; set; }
    }
}
