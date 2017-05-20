using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PicuCalendars.DataAccess
{
    public class Roster
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; }
        public string RosterName { get; set; }
        public virtual byte[] Secret { get; set; }
    }
}
