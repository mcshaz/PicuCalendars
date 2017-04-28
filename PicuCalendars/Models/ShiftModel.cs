using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.Models
{
    [Serializable]
    public class ShiftModel
    {
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string ShiftCode { get; set; }

        [EachStringLength(2,MinimumLength = 1)]
        public string[] StaffInitials { get; set; }
    }

    [Serializable]
    public class ShiftModelError
    {
        public enum ShiftErrorType { ShiftCodeNotFound, StaffInitialsNotFound }
        public ShiftModel ErrorModel { get; set; }
        public ShiftErrorType ErrorType {get; set;}
    }

    [Serializable]
    public class ShiftModelErrorCollection
    {
        public DataException DatabaseException { get; set; }
        public List<ShiftModelError> ModelErrors { get; set; }
    }
}
