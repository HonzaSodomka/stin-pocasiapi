using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace stin_backend.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
    }
}
