using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PicuCalendars.DataAccess
{
    public class Shift
    {
        public Guid RosterId { get; set; }
        public const int CodeLength = 2;
        [StringLength(CodeLength)]
        public string Code { get; set; }
        public const int DescriptionLength = 128;
        [StringLength(DescriptionLength)]
        public string Description { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public int DurationMins { get; set; }
        public bool LeaveShift { get; set; }

        [NotMapped]
        [JsonIgnore]
        public TimeSpan Duration
        {
            get { return new TimeSpan(0, DurationMins, 0); }
            set { DurationMins = (int)value.TotalMinutes; }
        }
    }
}
