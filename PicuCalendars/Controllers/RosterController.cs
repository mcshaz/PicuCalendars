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
    [Authorize] //note with current implementation, can never authorize
    [Route("api/[controller]")]
    public class RosterController : Controller
    {
        private readonly CalendarContext _context;

        public RosterController(CalendarContext context)
        {
            _context = context;
        }

        [HttpGet("{rosterId}")]
        public Roster Get(Guid rosterId)
        {
            return _context.Rosters.Find(rosterId);
        }

        [HttpGet]
        public IActionResult /*IEnumerable<Roster>*/ Get()
        {
            return new ObjectResult(_context.Rosters);
        }

        // POST api/values
        [Authorize(Policy = "CreateAtRoute")]
        [HttpPost("{rosterId}")]
        public IActionResult Post(Guid rosterId, [FromBody]Roster item)
        {
            if (item == null || item.Id != rosterId)
            {
                return BadRequest();
            }
            var serverRoster = ServerRoster.FromRoster(item);
            _context.Rosters.Add(serverRoster);
            _context.SaveChanges();
            return CreatedAtRoute(new { id = item.Id }, serverRoster);
        }

        // PUT api/values/5
        [Authorize(Policy = "UpdateAtRoute")]
        [HttpPut("{rosterId}")]
        public IActionResult Put(Guid rosterId, [FromBody]Roster item)
        {
            if (item == null ||item.Id != rosterId)
            {
                return BadRequest();
            }
            var existing = _context.Rosters.Find(rosterId);
            if (existing == null)
            {
                return NotFound();
            }
            var serverRoster = ServerRoster.FromRoster(item);
            _context.Entry(existing).CurrentValues.SetValues(serverRoster);
            _context.SaveChanges();
            return new NoContentResult();
        }

        // DELETE api/values/5
        [HttpDelete("{rosterId}")]
        public IActionResult Delete(Guid rosterId)
        {
            var item = _context.Rosters.Find(rosterId);
            if (item == null)
            {
                return NotFound();
            }
            _context.Rosters.Remove(item);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}
