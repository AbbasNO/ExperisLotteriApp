using Microsoft.EntityFrameworkCore;

namespace Server.Data
{
    public class LotteriDbContext : DbContext
    {
        public LotteriDbContext(DbContextOptions<LotteriDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
    }
}
