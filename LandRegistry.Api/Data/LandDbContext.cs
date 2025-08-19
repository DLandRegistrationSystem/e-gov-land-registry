using LandRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LandRegistry.Api.Data
{
    public class LandDbContext : DbContext
    {
        public LandDbContext(DbContextOptions<LandDbContext> options) : base(options) { }

        public DbSet<LandRecord> LandRecords { get; set; }
    }
}
