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
        [StringLength(128)]
        public string FullName { get; set; }
        public const int StaffMemberCodeLength = 32;
        public const int StaffMemberCodeMinLength = 1;
        [StringLength(StaffMemberCodeLength, MinimumLength = StaffMemberCodeMinLength)]
        public string StaffMemberCode { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public Guid RosterId { get; set; }
    }
}
