using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.Models
{
    [Serializable]
    public class Appointment
    {
        [JsonConverter(typeof(OnlyDateConverter))]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string ShiftCode { get; set; }

        [EachStringLength(64,MinimumLength = 1)]
        public string[] StaffInitials { get; set; }
    }

    [Serializable]
    public class ShiftModelError
    {
        public enum ShiftErrorType { ShiftCodeNotFound, StaffInitialsNotFound }
        public Appointment ErrorModel { get; set; }
        public ShiftErrorType ErrorType {get; set;}
    }

    [Serializable]
    public class ShiftModelErrorCollection
    {
        public DataException DatabaseException { get; set; }
        public List<ShiftModelError> ModelErrors { get; set; }
    }

    public class OnlyDateConverter : IsoDateTimeConverter
    {
        public OnlyDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
