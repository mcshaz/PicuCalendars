using Ical.Net;
using Ical.Net.DataTypes;
using PicuCalendars.DataAccess;
using PicuCalendars.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqKit;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Serialization;
using System.Diagnostics;

namespace PicuCalendars.Services
{
    public sealed class CalendarServices
    {
        private readonly CalendarContext _context;

        public CalendarServices(CalendarContext context)
        {
            _context = context;
        }

        public string GetCalendar(Guid staffId, int daysPriorToCommence = 14)
        {
            var calendar = new Calendar
            {
                Version = CalendarVersions.Latest,
                Method = CalendarMethods.Publish
            };

            var staffMember = _context.Staff.Include("Department")
                .Single(s => s.Id == staffId);
            staffMember.LastViewedVersionId = _context.Versions.Max(v=>v.Number);
            foreach (var e in GetCalls(staffMember, daysPriorToCommence))
            {
                calendar.Events.Add(e);
            }
            _context.SaveChanges();
            var serializer = new CalendarSerializer(new SerializationContext());
            return serializer.SerializeToString(calendar);
        }

        public IEnumerable<Event> GetCalls(StaffMember staffMember, int daysPriorToCommence = 14)
        {
            var datePrior = DateTime.Today - TimeSpan.FromDays(daysPriorToCommence);
            var dpt = staffMember.Department;

            var userAppointments = (from a in _context.Appointments
                                        .Include("VersionCreated").Include("VersionCancelled")
                                        .AsNoTracking() 
                                    where a.StaffMemberId == staffMember.Id && a.Start > datePrior
                                    orderby a.Start
                                    select a).ToLookup(a => new { Cancelled = a.VersionCancelledId != null, a.IsLeaveShift, a.Description });


            List<Event> leave = new List<Event>();
            List<Event> cancelled = new List<Event>();
            List<Event> shifts = new List<Event>();

            foreach (var g in userAppointments)
            {
                if (g.Key.Cancelled)
                {
                    cancelled.AddRange(MapCancelled(g));
                }
                else if(g.Key.IsLeaveShift)
                {
                    leave.AddRange(MapLeave(g));
                }
                else
                {
                    shifts.AddRange(MapCalls(g));
                }
            }

            shifts.Sort(SortEvents);
            var datePred = PredicateBuilder.New<Appointment>();
            foreach (var s in shifts.ConsecutiveGroup((prior, curr)=>curr.DtStart.AsSystemLocal <= prior.DtEnd.AsSystemLocal))
            {
                var start = s[0].DtStart.AsSystemLocal;
                var finish = s[s.Count - 1].DtEnd.AsSystemLocal;
                datePred = datePred.Or(a => a.Start < finish && a.Finish > start);
            }

            var colleagues =(_context.Appointments
                                .Include("VersionCreated")
                                .Include("StaffMember")
                                .AsNoTracking()
                                .Where(a => a.VersionCancelledId == null && !a.IsLeaveShift && a.StaffMemberId != staffMember.Id)
                                .Where(datePred)
                                .OrderBy(a=>a.Start)
                                .ToList());

            var debugColleagues = new List<Appointment>();
            var badDate = new DateTime(2017, 5, 8, 8, 0, 0);
            foreach (var s in shifts.ConsecutiveGroup((prior, curr) => curr.DtStart.AsSystemLocal <= prior.DtEnd.AsSystemLocal))
            {
                var start = s[0].DtStart.AsSystemLocal;
                var finish = s[s.Count - 1].DtEnd.AsSystemLocal;
                if (colleagues.Any(a => a.Start < finish && a.Finish > start && a.Start == badDate))
                {
                    Console.WriteLine(badDate);
                }
            }

            int colleagueIndex = 0;
            foreach (var s in shifts)
            {
                DateTime shiftFinish = s.DtEnd.AsSystemLocal;
                int length = 0;
                int startIndex = colleagueIndex;
                while (colleagueIndex < colleagues.Count && colleagues[colleagueIndex].Start < shiftFinish)
                {
                    ++length;
                    ++colleagueIndex;
                }
                var shiftColleagues = colleagues.GetRange(startIndex, length);
                Debug.Assert(shiftColleagues.All(c => c.Finish > s.DtStart.AsSystemLocal && c.Start < s.DtEnd.AsSystemLocal),"colleagues working outside date range");
                
                UpdateEventWithColleaguesData(s, shiftColleagues);
                if (colleagueIndex >= colleagues.Count)
                {
                    break;
                }
            }
            Debug.Assert(colleagueIndex >= colleagues.Count, "All colleagues not assigned");
            return shifts.Concat(leave).Concat(cancelled).ToList();
        }
        private static IEnumerable<Event> MapEvent(IEnumerable<Appointment> events, Action<Event, IList<Appointment>> pred)
        {
            return events.ConsecutiveGroup((prior, current) => current.Start <= prior.Finish)
                .Select(ge =>
                {
                    DateTime start = ge[0].Start;
                    DateTime finish = ge[ge.Count - 1].Finish;
                    var returnVar = new Event
                    {
                        Uid = GetHash(ge.Select(e => e.Id)),
                        Created = new CalDateTime(ge.Min(e => e.VersionCreated.Created)),
                        Sequence = ge.Max(e => e.VersionCreatedId),
                        DtStart = new CalDateTime(start),
                        DtEnd = new CalDateTime(finish),
                        IsAllDay = start.TimeOfDay.Ticks == 0 && finish.TimeOfDay.Ticks == 0,
                        Summary = GetSummary(ge[0])
                    };
                    pred(returnVar, ge);
                    return returnVar;
                });
        }

        internal static string GetSummary(Appointment evt)
        {
            return evt.Department.Name + ' ' + evt.Description;
        }

        private static IEnumerable<Event> MapLeave(IEnumerable<Appointment> events)
        {
            return MapEvent(events, (evt, appts) => {
                evt.Status = EventStatus.Confirmed;
                evt.LastModified = new CalDateTime(appts.Max(e => e.VersionCreated.Created));
            });
        }

        private static IEnumerable<Event> MapCancelled(IEnumerable<Appointment> events)
        {
            return MapEvent(events, (evt, appts) => {
                var minCancelDate = appts.Min(e => e.VersionCancelled.Created);
                var maxCancelDate = appts.Max(e => e.VersionCancelled.Created);
                evt.Status = EventStatus.Cancelled;
                evt.LastModified = new CalDateTime(maxCancelDate);
                evt.Description = $"Created {evt.DtStart.Value:d} & Cancelled {minCancelDate:d}"
                    + (minCancelDate == maxCancelDate
                        ? string.Empty
                        : '-' + maxCancelDate.ToShortDateString());
            });
        }

        private static IEnumerable<Event> MapCalls(IEnumerable<Appointment> events)
        {
            return MapEvent(events, (evt, appts) => {
                evt.Status = EventStatus.Confirmed;
                evt.LastModified = new CalDateTime(appts.Max(e => e.VersionCreated.Created));
            });
        }

        private static string GetHash(IEnumerable<int> ints)
        {
            var byteArray = ints.SelectMany(i => BitConverter.GetBytes(i)).ToArray();
            var hash = Farmhash.Sharp.Farmhash.Hash64(byteArray, byteArray.Length);
            return hash.ToString();
            //could try http://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html
        }

        private static int SortEvents(Event x, Event y)
        {
            return x.DtStart.AsSystemLocal.CompareTo(y.DtEnd.AsSystemLocal);
        }

        private static void UpdateEventWithColleaguesData(Event evt, IEnumerable<Appointment> colleagues)
        {
            if (!colleagues.Any()) { return; }
            List<ColleagueModel> sameCallPrincipal = new List<ColleagueModel>();
            List<ColleagueOtherShiftModel> otherCalls = new List<ColleagueOtherShiftModel>();
            foreach (var g in colleagues.ToLookup(c => new { c.StaffMember, Summary = GetSummary(c) }))
            {
                var consGrp = g.ConsecutiveGroup((prior, cur) => cur.Start <= prior.Finish);
                if (g.Key.Summary == evt.Summary)
                {
                    sameCallPrincipal.AddRange(consGrp.Select(ColleagueModel.FromAppointments));
                }
                else
                {
                    otherCalls.AddRange(consGrp.Select(ColleagueOtherShiftModel.FromAppointments));
                }
            }

            evt.Sequence = (new[] { evt.Sequence }).Concat(sameCallPrincipal.Concat(otherCalls).Select(c => c.Sequence)).Max();
            var maxDate = sameCallPrincipal.Concat(otherCalls).Max(c => c.Modified);
            if (maxDate > evt.LastModified.AsSystemLocal)
            {
                evt.LastModified = new CalDateTime(maxDate);
            }

            var desc = new StringBuilder(evt.Description);
            if (sameCallPrincipal.Any())
            {
                var summ = new StringBuilder(evt.Summary + " (with ");
                bool appendTab = false;
                const string sep = ", ";
                foreach (var s in sameCallPrincipal)
                {
                    if (appendTab)
                    {
                        desc.Append("\r\n\twith ");
                    }
                    else
                    {
                        desc.Append(" with ");
                        appendTab = true;
                    }
                    desc.Append(s.Person.FullName);
                    summ.Append(s.Person.Abbreviation);
                    if (s.Start > evt.DtStart.AsSystemLocal)
                    {
                        string st = $" from {s.Start:g}";
                        desc.Append(st);
                        summ.Append(st);
                    }
                    if (s.Finish < evt.DtEnd.AsSystemLocal)
                    {
                        string fn = s.FinishString();
                        desc.Append(" until " + fn);
                        summ.Append(" to " + fn);
                    }
                    summ.Append(sep);
                    desc.AppendLine();
                }
                summ.Length -= sep.Length;
                summ.Append(')');
                evt.Summary = summ.ToString();
            }

            foreach (var o in otherCalls)
            {
                desc.Append(o.Description + ' ' + o.Person.FullName);
                if (o.Start > evt.DtStart.AsSystemLocal)
                {
                    desc.Append($" from {o.Start:g}");
                }
                if (o.Finish < evt.DtEnd.AsSystemLocal)
                {
                    desc.Append($" until {o.FinishString()}");
                }
                desc.AppendLine();
            }
            evt.Description = desc.ToString();

        }

        private class ColleagueModel
        {
            public StaffMember Person { get; set; }
            public DateTime Start { get; set; }
            public DateTime Finish { get; set; }
            public int Sequence { get; set; }
            public DateTime Modified { get; set; }

            public string FinishString()
            {
                return Finish.ToString(Finish.Date == Start.Date ? "t" : "g");
            }

            public string DateRangeString()
            {
                return Start.ToString("g") + " - " + FinishString();
            }

            protected static void InstantiateColleagueModel(IList<Appointment> appts, ColleagueModel model)
            {
                model.Person = appts[0].StaffMember;
                model.Start = appts[0].Start;
                model.Finish = appts[appts.Count - 1].Finish;
                model.Sequence = appts.Max(e => e.VersionCreatedId);
                model.Modified = appts.Max(e => e.VersionCreated.Created);
            }

            public static ColleagueModel FromAppointments(IList<Appointment> appts)
            {
                var returnVar = new ColleagueModel();
                InstantiateColleagueModel(appts, returnVar);
                return returnVar;
            }
        }

        private class ColleagueOtherShiftModel : ColleagueModel
        {
            public string Description { get; set; }

            public static new ColleagueOtherShiftModel FromAppointments(IList<Appointment> appts)
            {
                var returnVar = new ColleagueOtherShiftModel
                {
                    Description = appts[0].Description,
                };
                InstantiateColleagueModel(appts, returnVar);
                return returnVar;
            }
        }
    }

}
