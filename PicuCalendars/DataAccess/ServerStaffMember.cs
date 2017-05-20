﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class ServerStaffMember : StaffMember
    {
        public virtual ICollection<ServerAppointment> Appointments { get; set; }

        public int? LastViewedVersionId { get; set; }
        public virtual CalendarVersion LastViewedVersion { get; set; }
        public virtual ServerRoster Roster { get; set; }

        internal static ServerStaffMember FromStaffMember(StaffMember staff)
        {
            return new ServerStaffMember
            {
                Email = staff.Email,
                FullName = staff.FullName,
                Id = staff.Id,
                RosterCode = staff.RosterCode,
                RosterId = staff.RosterId
            };
        }
    }
}
