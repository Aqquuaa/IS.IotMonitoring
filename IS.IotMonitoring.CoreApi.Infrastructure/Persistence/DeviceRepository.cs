using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IS.IotMonitoring.CoreApi.Infrastructure.Persistence
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DeviceDbContext context;
        public DeviceRepository(DeviceDbContext context)
        {
            this.context = context;
        }
        public async Task<Device?> GetByIdAsync(Guid id)
        {
            return await context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IReadOnlyCollection<Device>> GetAllAsync()
        {
            return await context.Devices.ToListAsync();
        }

        public async Task AddAsync(Device device)
        {
            await context.Devices.AddAsync(device);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Device device)
        {
            context.Devices.Update(device);
            await context.SaveChangesAsync();
        }
    }
}
