using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Statistic
        [HttpGet]
        public async Task<ActionResult<IEnumerable<statistic>>> Getstatistics()
        {
            return await _context.statistics.ToListAsync();
        }

        // GET: api/Statistic/5
        [HttpGet("{id}")]
        public async Task<ActionResult<statistic>> Getstatistic(int id)
        {
            var statistic = await _context.statistics.FindAsync(id);

            if (statistic == null)
            {
                return NotFound();
            }

            return statistic;
        }

        // PUT: api/Statistic/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putstatistic(int id, statistic statistic)
        {
            if (id != statistic.statistic_id)
            {
                return BadRequest();
            }

            _context.Entry(statistic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!statisticExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Statistic
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<statistic>> Poststatistic(statistic statistic)
        {
            _context.statistics.Add(statistic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getstatistic", new { id = statistic.statistic_id }, statistic);
        }

        // DELETE: api/Statistic/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletestatistic(int id)
        {
            var statistic = await _context.statistics.FindAsync(id);
            if (statistic == null)
            {
                return NotFound();
            }

            _context.statistics.Remove(statistic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool statisticExists(int id)
        {
            return _context.statistics.Any(e => e.statistic_id == id);
        }
    }
}
