using LandRegistry.Api.Data;
using LandRegistry.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandRegistry.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LandRecordsController : ControllerBase
    {
        private readonly LandDbContext _context;

        public LandRecordsController(LandDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LandRecord>>> GetRecords()
        {
            return await _context.LandRecords.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<LandRecord>> AddRecord(LandRecord record)
        {
            _context.LandRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecords), new { id = record.Id }, record);
        }
    }
}
