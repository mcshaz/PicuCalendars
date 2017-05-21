using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public partial class StaffMember
    {
        //the Guid is almost a securty token for downloading calendar - could use departmentId and Abbreviation otherwise
        //public Guid Id { get; set; }
        public string FullName { get; set; }
        [StringLength(32, MinimumLength = 1)]
        public string RosterCode { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public Guid RosterId { get; set; }
    }
}
