using LinqKit;
using PicuCalendars.DataAccess;
using PicuCalendars.Utilities;
using PicuCalendars.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PicuCalendars.Services
{
    public class DayServices
    {
        private static readonly TimeSpan _dayStart = TimeSpan.FromHours(8);
        private static readonly TimeSpan _dayDuration = _dayStart;
        private static readonly TimeSpan _eveningStart = TimeSpan.FromHours(16);
        private static readonly TimeSpan _eveningDuration = _eveningStart;
        private static readonly TimeSpan _leaveDuration = TimeSpan.FromHours(24);

        public DayServices(CalendarContext context)
        {
            _context = context;
        }

        public ShiftModelErrorCollection UpsertAppointments(IEnumerable<Appointment> dayModels, Guid rosterId, bool createStaff = false)
        {
            var returnVar = new ShiftModelErrorCollection
            {
                ModelErrors = new List<ShiftModelError>()
            };
            if (!dayModels.Any()) { return returnVar; }

            CalendarVersion _version = null;
            var getVersion = new Func<CalendarVersion>(() => {
                if (_version == null) {
                    _version = new CalendarVersion
                    {
                        Created = DateTime.Now
                    };
                    _context.Versions.Add(_version);
                }
                return _version;
            });

            var staff = _context.Staff.Where(s => s.RosterId == rosterId).ToDictionary(s=>s.StaffMemberCode);
            var shifts = _context.Shifts.Where(s => s.RosterId == rosterId).ToDictionary(s=>s.Code);

            //to do concurrency/lock rows
            //https://msdn.microsoft.com/en-us/library/dn456843(v=vs.113).aspx

            var lastViewedVersionId = staff.Values.Max(s => s.LastViewedVersionId) ?? int.MinValue;

            var newAppointments = dayModels.SelectMany(dms => {
                if(shifts.TryGetValue(dms.ShiftCode, out ServerShift shift))
                {
                    var startDate = dms.Date + shift.ShiftStart;
                    var finishDate = startDate + shift.Duration;
                    var apptList = new List<ServerAppointment>(dms.StaffInitials.Length);
                    foreach (var si in dms.StaffInitials)
                    {
                        if (staff.TryGetValue(si, out ServerStaffMember sm))
                        {
                            apptList.Add(new ServerAppointment
                            {
                                Start = startDate,
                                Finish = finishDate,
                                Staff = sm,
                                RosterId = rosterId,
                                Description = shift.Description,
                                IsLeaveShift = shift.LeaveShift
                            });
                        }
                        else if (createStaff)
                        {
                            var newStaff = new ServerStaffMember { StaffMemberCode = si, RosterId = rosterId };
                            _context.Staff.Add(newStaff);
                            staff.Add(si, newStaff);
                        }
                        else
                        {
                            //_context.Staff.Add(new ServerStaffMember { RosterId = rosterId, RosterCode = si });
                            returnVar.ModelErrors.Add(new ShiftModelError {
                                 ErrorType = ShiftModelError.ShiftErrorType.StaffInitialsNotFound,
                                 ErrorModel = dms
                            });
                        }
                    }
                    return apptList;
                }
                else
                {
                    returnVar.ModelErrors.Add(new ShiftModelError
                    {
                        ErrorModel = dms,
                        ErrorType = ShiftModelError.ShiftErrorType.ShiftCodeNotFound
                    });
                    return Enumerable.Empty<ServerAppointment>();
                }
            }).ToDictionary(a=> new { a.Start, a.Staff.StaffMemberCode, a.Description });

            var existingAppointments = _context.Appointments
                .Where(ContainsDates(newAppointments.Values.Select(a=>a.Start)))
                .Where(a => a.VersionCancelledId == null && rosterId == a.RosterId)
                .ToList();

            //logic:
            //existing date/description/staffmember not matched to newly created in same date range -> delete or set VersionCancelled
            //is matched - do nothing
            //remainder - add

            foreach (var existAppt in existingAppointments)
            {
                
                if (!newAppointments.Remove(new { existAppt.Start, existAppt.Staff.StaffMemberCode, existAppt.Description }))
                {
                    if (lastViewedVersionId >= existAppt.VersionCreatedId)
                    {
                        existAppt.VersionCancelled = getVersion();
                    }
                    else
                    {
                        _context.Appointments.Remove(existAppt);
                    }
                }
            }
            foreach (var newAppt in newAppointments.Values)
            {
                newAppt.VersionCreated = getVersion();
                _context.Appointments.Add(newAppt);
            }
            try
            {
                _context.SaveChanges();
            }
            catch (System.Data.DataException e)
            {
                returnVar.DatabaseException = e;
            }
            return returnVar;
        }

        private readonly CalendarContext _context;

        public static Expression<Func<ServerAppointment, bool>> ContainsDates(IEnumerable<DateTime> dates)
        {
            var predicate = PredicateBuilder.New<ServerAppointment>();
            foreach (var d in DateRange.FromDates(dates))
            {
                predicate = predicate.Or(p => d.Start <= p.Start && p.Start <= d.Finish);
            }
            return predicate;
        }
    }
}
