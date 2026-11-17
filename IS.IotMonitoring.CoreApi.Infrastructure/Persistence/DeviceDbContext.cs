using IS.IotMonitoring.CoreApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IS.IotMonitoring.CoreApi.Infrastructure.Persistence
{
    public class DeviceDbContext : DbContext
    {
        public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options) { }

        public DbSet<Device> Devices => Set<Device>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(builder => 
            {
                builder.ToTable("devices");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name).IsRequired().HasMaxLength(128);

                builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(50);

                builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(50);
            });
        }
    }
}
