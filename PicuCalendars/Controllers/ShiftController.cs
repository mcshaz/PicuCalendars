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
    public class ShiftController : Controller
    {
        private readonly CalendarContext _context;

        public ShiftController(CalendarContext context)
        {
            _context = context;
        }
        // GET: api/values
        [HttpGet("{RosterId}")]
        public IEnumerable<Shift> Get(Guid rosterId)
        {
            return _context.Shifts.Where(s=>s.RosterId == rosterId);
        }

        [HttpGet("{rosterId}/{code}")]
        public IActionResult Get(Guid rosterId, string code)
        {
            var item = _context.Shifts.Find(rosterId, code);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST api/values
        [HttpPost("rosterId")]
        public IActionResult Post(Guid rosterId, [FromBody]Shift item)
        {
            if (item == null || item.RosterId != rosterId)
            {
                return BadRequest();
            }
            _context.Shifts.Add(item);
            _context.SaveChanges();
            return CreatedAtRoute(new { item.RosterId, item.Code}, item);
        }

        // PUT api/values/5
        [HttpPut("{rosterId}/{code}")]
        public IActionResult Put(Guid rosterId, string code, [FromBody]Shift item)
        {
            if (item == null || item.Code != code || item.RosterId != rosterId)
            {
                return BadRequest();
            }
            var existing = _context.Shifts.Find(rosterId, code);
            if (existing == null)
            {
                return NotFound();
            }
            _context.Entry(existing).CurrentValues.SetValues(item);
            _context.SaveChanges();
            return new NoContentResult();
        }

        // DELETE api/values/5
        [HttpDelete("{rosterId}/{code}")]
        public IActionResult Delete(Guid rosterId, string code)
        {
            var shift = _context.Shifts.Find(rosterId, code);
            if (shift == null)
            {
                return NotFound();
            }
            _context.Shifts.Remove(shift);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}
