using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Domain.Entities;
using MongoDB.Driver;

namespace IS.IotMonitoring.CoreApi.Infrastructure.Persistence
{
    public class TelemetryRepository : ITelemetryRepository
    {
        private readonly IMongoCollection<TelemetryReading> telemetry;
        public TelemetryRepository(IMongoDatabase database)
        {
            telemetry = database.GetCollection<TelemetryReading>("telemetry");
        }
        public async Task AddAsync(TelemetryReading reading)
        {
            await telemetry.InsertOneAsync(reading);
        }

        public async IAsyncEnumerable<TelemetryReading> GetByDeviceIdAsync(Guid deviceId, DateTimeOffset? from, DateTimeOffset? to)
        {
            var builder = Builders<TelemetryReading>.Filter;
            var filter = builder.Eq(t => t.DeviceId, deviceId);

            if (from.HasValue)
                filter &= builder.Gte(t => t.Timestamp, from.Value);
            if (to.HasValue)
                filter &= builder.Lte(t => t.Timestamp, to.Value);

            using var cursor = await telemetry.Find(filter).SortByDescending(t => t.Timestamp).ToCursorAsync();

            while (await cursor.MoveNextAsync())
            {
                foreach (var document in cursor.Current)
                {
                    yield return document;
                }
            }
        }
    }
}
