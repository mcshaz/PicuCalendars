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

        [HttpGet("{id}")]
        public Roster Get(Guid id)
        {
            return _context.Rosters.Find(id);
        }

        [HttpGet]
        public IEnumerable<Roster> Get()
        {
            return _context.Rosters;
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Roster item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            _context.Rosters.Add(ServerRoster.FromRoster(item));
            _context.SaveChanges();
            return CreatedAtRoute(new { item.Id}, item);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id,[FromBody]Roster item)
        {
            if (item == null ||item.Id != id)
            {
                return BadRequest();
            }
            var existing = _context.Rosters.Find(id);
            if (existing == null)
            {
                return NotFound();
            }
            _context.Entry(existing).CurrentValues.SetValues(item);
            _context.SaveChanges();
            return new NoContentResult();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var item = _context.Rosters.Find(id);
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
