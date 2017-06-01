using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class ServerShift : Shift
    {
        public virtual ServerRoster Roster { get; set; }

        internal static ServerShift FromShift(Shift shift)
        {
            return new ServerShift
            {
                Code = shift.Code,
                Description = shift.Description,
                DurationMins = shift.DurationMins,
                LeaveShift = shift.LeaveShift,
                RosterId = shift.RosterId,
                ShiftStart = shift.ShiftStart
            };

        }
    }
}
