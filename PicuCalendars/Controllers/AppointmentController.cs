using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using PicuCalendars.Services;
using PicuCalendars.Security;
using Microsoft.AspNetCore.Authorization;

namespace PicuCalendars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AppointmentController : Controller
    {
        private readonly CalendarContext _context;

        public AppointmentController(CalendarContext context)
        {
            _context = context;
        }
        // GET api/calendar/123-ad-
        [AllowAnonymous]
        [HttpGet("{rosterId}/{staffInitials}")]
        public string Calendar(Guid rosterId, string staffInitials)
        {
            var cs = new CalendarServices(_context);
            return cs.GetCalendar(rosterId, staffInitials);
        }

        [HttpPost("{rosterId}")]
        [Authorize(Policy = "UpdateAtRoute")]
        public IActionResult Post(Guid rosterId,[FromBody]List<Appointment> days)
        {
            if (days == null)
            {
                return BadRequest();
            }
            //could eventually save size by using - http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
            var ds = new DayServices(_context);
            return new ObjectResult(ds.UpsertAppointments(days, rosterId));
        }
    }
}
