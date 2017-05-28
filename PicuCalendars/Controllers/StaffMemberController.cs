using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PicuCalendars.DataAccess;
using EFExtensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PicuCalendars.Controllers
{
    [Authorize(Policy = "UpdateAtRoute")]
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
        [AllowAnonymous]
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
        [HttpPost("{rosterId}/{rosterCode}")]
        public IActionResult Post(Guid rosterId, string rosterCode,[FromBody]StaffMember item)
        {
            if (item == null || item.RosterId != rosterId || item.RosterCode != rosterCode)
            {
                return BadRequest();
            }
            var serverStaffMember = ServerStaffMember.FromStaffMember(item);
            _context.Staff.Add(serverStaffMember);
            _context.SaveChanges();
            return CreatedAtRoute(new { item.RosterId, item.RosterCode }, serverStaffMember);
        }

        [HttpPost("{rosterId}")]
        public IActionResult Post(Guid rosterId,[FromBody]IReadOnlyList<StaffMember> items)
        {
            if (items == null || items.Any(i=> i.RosterId != rosterId))
            {
                return BadRequest();
            }
            var serverStaffMembers = items.Select(ServerStaffMember.FromStaffMember).ToList();
            _context
                .Upsert(serverStaffMembers)
                .Execute();
            //_context.SaveChanges();
            return CreatedAtRoute(new { rosterId }, serverStaffMembers);
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
            var serverStaffMember = ServerStaffMember.FromStaffMember(item);
            _context.Entry(existing).CurrentValues.SetValues(serverStaffMember);
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
