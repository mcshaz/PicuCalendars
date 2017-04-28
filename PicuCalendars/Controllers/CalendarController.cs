using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using PicuCalendars.Services;

namespace PicuCalendars.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private readonly CalendarContext _context;

        public CalendarController(CalendarContext context)
        {
            _context = context;
        }
        // GET api/calendar/123-ad-
        [HttpGet("{id}")]
        public string Calendar(Guid id)
        {
            var cs = new CalendarServices(_context);
            return cs.GetCalendar(id);
        }

        [HttpPost("{departmentId}")]
        public ShiftModelErrorCollection Post(Guid departmentId,[FromBody]List<ShiftModel> days)
        {
            //could eventually save size by using - http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
            var ds = new DayServices(_context);
            return ds.UpsertAppointments(days, departmentId);
        }
    }
}
