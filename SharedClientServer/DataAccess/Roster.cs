using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PicuCalendars.DataAccess
{
    public class Roster
    {
        public Guid Id { get; set; }
        [StringLength(64)]
        public string DepartmentName { get; set; }
        [StringLength(64)]
        public string RosterName { get; set; }
        [MaxLength(64),MinLength(64)]
        public virtual byte[] Secret { get; set; }
    }
}
