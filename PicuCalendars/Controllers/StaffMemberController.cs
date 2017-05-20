using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PicuCalendars.DataAccess;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PicuCalendars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class StaffMemberController : Controller
    {
        private readonly CalendarContext _context;

        public StaffMemberController(CalendarContext context)
        {
            _context = context;
        }
        // GET: api/values
        [HttpGet("{rosterId}")]
        public IEnumerable<StaffMember> Get(Guid rosterId)
        {
            return _context.Staff.Where(s => s.RosterId == rosterId);
        }

        [HttpGet("{rosterId}/{abbreviation}")]
        public IActionResult Get(Guid rosterId, string abbreviation)
        {
            var item = _context.Staff.Find(rosterId, abbreviation);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST api/values
        [HttpPost("rosterId")]
        public IActionResult Post(Guid rosterId, [FromBody]StaffMember item)
        {
            if (item == null || item.RosterId != rosterId)
            {
                return BadRequest();
            }
            _context.Staff.Add(ServerStaffMember.FromStaffMember(item));
            _context.SaveChanges();
            return CreatedAtRoute(new { item.RosterId, item.RosterCode }, item);
        }

        [HttpPost("rosterId")]
        public IActionResult Post(Guid rosterId, [FromBody]List<StaffMember> items)
        {
            if (items == null || items.Any(i=> i.RosterId != rosterId))
            {
                return BadRequest();
            }
            _context.Staff.AddRange(items.Select(ServerStaffMember.FromStaffMember));
            _context.SaveChanges();
            return CreatedAtRoute(new { rosterId }, items);
        }

        // PUT api/values/5
        [HttpPut("{rosterId}/{abbreviation}")]
        public IActionResult Put(Guid rosterId, string abbreviation, [FromBody]StaffMember item)
        {
            if (item == null || item.RosterCode != abbreviation || item.RosterId != rosterId)
            {
                return BadRequest();
            }
            var existing = _context.Staff.Find(rosterId, abbreviation);
            if (existing == null)
            {
                return NotFound();
            }
            _context.Entry(existing).CurrentValues.SetValues(item);
            _context.SaveChanges();
            return new NoContentResult();
        }

        // DELETE api/values/5
        [HttpDelete("{rosterId}/{abbreviation}")]
        public IActionResult Delete(Guid rosterId, string abbreviation)
        {
            var item = _context.Staff.Find(rosterId, abbreviation);
            if (item == null)
            {
                return NotFound();
            }
            _context.Staff.Remove(item);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}
