using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader
{
    class ValidateAttributes
    {
        public static bool IsValid(object o, TextWriter error)
        {
            var context = new ValidationContext(o, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            if (Validator.TryValidateObject(o, context, results, true))
            {
                return true;
            }
            foreach (var r in results)
            {
                error.WriteLine($"{string.Join(",", r.MemberNames)}:{r.ErrorMessage}");
            }
            return false;
        }
    }
}
