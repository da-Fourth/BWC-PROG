using Microsoft.EntityFrameworkCore;
using BWC.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BWC.DataConnection
{
    public class SqlServerDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API configurations (if any)
            base.OnModelCreating(modelBuilder);
        }
    }
}
