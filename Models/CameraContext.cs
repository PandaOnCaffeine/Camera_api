using System.Collections.Generic;
using System.Reflection.Emit;

using Camera_api.Models.Returns;
using Microsoft.EntityFrameworkCore;

namespace Camera_api.Models
{
    public class CameraContext : DbContext
    {
        IConfiguration _configuration;
        public CameraContext(DbContextOptions<CameraContext> options, IConfiguration Configuration) : base(options)
        {
            _configuration = Configuration;
        }
        //"Server=localhost\\SQLEXPRESS;Database=Lagerstyringssystem;Trusted_Connection=true;TrustServerCertificate=true;"
        //"Server=localhost\\MSSQLSERVER03;Database=Lagerstyringssystem;Trusted_Connection=true;TrustServerCertificate=true;"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_configuration.GetConnectionString("YourConnectionString"));

        public DbSet<ImageReturn> ImageReturns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageReturn>().HasNoKey();

        }
    }
}
