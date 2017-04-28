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

        public ShiftModelErrorCollection UpsertAppointments(IEnumerable<ShiftModel> dayModels, Guid departmentId)
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

            var staff = _context.Staff.Where(s => s.DepartmentId == departmentId).ToDictionary(s=>s.Abbreviation);
            var shifts = _context.Shifts.Where(s => s.DepartmentId == departmentId).ToDictionary(s=>s.Code);

            //to do concurrency/lock rows
            //https://msdn.microsoft.com/en-us/library/dn456843(v=vs.113).aspx

            var lastViewedVersionId = staff.Values.Max(s => s.LastViewedVersionId) ?? int.MinValue;

            var newAppointments = dayModels.SelectMany(dms => {
                if(shifts.TryGetValue(dms.ShiftCode, out Shift shift))
                {
                    var startDate = dms.Date + shift.ShiftStart;
                    var finishDate = startDate + shift.Duration;
                    var apptList = new List<Appointment>(dms.StaffInitials.Length);
                    foreach (var si in dms.StaffInitials)
                    {
                        if (staff.TryGetValue(si, out StaffMember sm))
                        {
                            apptList.Add(new Appointment
                            {
                                Start = startDate,
                                Finish = finishDate,
                                StaffMember = sm,
                                DepartmentId = departmentId,
                                Description = shift.Description,
                                IsLeaveShift = shift.LeaveShift
                            });
                        }
                        else
                        {
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
                    return Enumerable.Empty<Appointment>();
                }
            }).ToDictionary(a=> new { a.Start, a.StaffMember.Abbreviation, a.Description });

            var existingAppointments = _context.Appointments
                .Where(ContainsDates(newAppointments.Values.Select(a=>a.Start)))
                .Where(a => a.VersionCancelledId == null && departmentId == a.DepartmentId)
                .ToList();

            //logic:
            //existing date/description/staffmember not matched to newly created in same date range -> delete or set VersionCancelled
            //is matched - do nothing
            //remainder - add

            foreach (var existAppt in existingAppointments)
            {
                
                if (!newAppointments.Remove(new { existAppt.Start, existAppt.StaffMember.Abbreviation, existAppt.Description }))
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

        public static Expression<Func<Appointment, bool>> ContainsDates(IEnumerable<DateTime> dates)
        {
            var predicate = PredicateBuilder.New<Appointment>();
            foreach (var d in DateRange.FromDates(dates))
            {
                predicate = predicate.Or(p => d.Start <= p.Start && p.Start <= d.Finish);
            }
            return predicate;
        }
    }
}
