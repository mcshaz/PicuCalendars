using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.Utilities
{
    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }

        public static IEnumerable<DateRange> FromDates(IEnumerable<DateTime> inDates)
        {
            var sortedDates = inDates.OrderBy(d => d);
            DateRange currentRange = null;
            foreach (var d in inDates)
            {
                if (currentRange == null)
                {
                    currentRange = new DateRange { Start = d, Finish = d };
                }
                else if ((d - currentRange.Finish).Days > 1)
                {
                    yield return currentRange;
                    currentRange = new DateRange { Start = d, Finish = d };
                }
                else
                {
                    currentRange.Finish = d;
                }
            }
            
            if (currentRange != null)
            {
                yield return currentRange;
            }
        }
    }
}
