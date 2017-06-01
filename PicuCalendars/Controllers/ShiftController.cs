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
    public class ShiftController : Controller
    {
        private readonly CalendarContext _context;

        public ShiftController(CalendarContext context)
        {
            _context = context;
        }
        // GET: api/values
        [HttpGet("{rosterId}")]
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
        [HttpPost("{rosterId}/{code}")]
        public IActionResult Post(Guid rosterId, string code, [FromBody]Shift item)
        {
            if (item == null || item.RosterId != rosterId || item.Code != code)
            {
                return BadRequest();
            }
            _context.Shifts.Add(ServerShift.FromShift(item));
            _context.SaveChanges();
            return CreatedAtRoute(new { item.RosterId, item.Code}, item);
        }

        [HttpPost("{rosterId}")]
        public IActionResult Post(Guid rosterId, [FromBody]IReadOnlyList<Shift> items)
        {
            if (items == null || items.Any(i => i.RosterId != rosterId))
            {
                return BadRequest();
            }
            _context.Upsert(items.Select(ServerShift.FromShift))
                .Execute();
            return CreatedAtRoute(new { rosterId }, items);
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
            _context.Entry(existing).CurrentValues.SetValues(ServerShift.FromShift(item));
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
