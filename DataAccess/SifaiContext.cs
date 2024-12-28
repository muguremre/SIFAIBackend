using Microsoft.EntityFrameworkCore;
using SIFAIBackend.Entities;
using System.Collections.Generic;

namespace SIFAIBackend.DataAccess
{
    public class SifaiContext : DbContext
    {
        public SifaiContext(DbContextOptions<SifaiContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
