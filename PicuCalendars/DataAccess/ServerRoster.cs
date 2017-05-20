using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PicuCalendars.DataAccess
{
    public partial class ServerRoster : Roster
    {
        [JsonIgnore]
        public override byte[] Secret { get
            { return base.Secret; }
            set { base.Secret = value; } }

        public virtual ICollection<ServerAppointment> Appointments { get; set; }
        public virtual ICollection<ServerStaffMember> Staff { get; set; }

        internal static ServerRoster FromRoster(Roster roster)
        {
            return new ServerRoster
            {
                DepartmentName = roster.DepartmentName,
                Id = roster.Id,
                RosterName = roster.RosterName,
                Secret = roster.Secret
            };
        }
    }
}
