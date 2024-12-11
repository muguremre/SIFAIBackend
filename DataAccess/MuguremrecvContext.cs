using Microsoft.EntityFrameworkCore;
using SIFAIBackend.Entities;

namespace SIFAIBackend.DataAccess
{
    public class MuguremrecvContext : DbContext
    {
        public MuguremrecvContext(DbContextOptions<MuguremrecvContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
