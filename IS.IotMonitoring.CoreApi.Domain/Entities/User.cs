namespace IS.IotMonitoring.CoreApi.Domain.Entities
{
    public class User
    {
        private readonly HashSet<Guid> allowedDeviceIds = [];
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? RefreshToken { get; private set; }
        public IReadOnlyCollection<Guid> AllowedDeviceIds => allowedDeviceIds;

        private User() { }
        public static User Create(Guid id, string name, IEnumerable<Guid>? initialAllowedDeviceIds = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var user = new User
            {
                Id = id,
                Name = name,
            };

            if (initialAllowedDeviceIds is not null)
            {
                foreach (var deviceId in initialAllowedDeviceIds)
                {
                    user.allowedDeviceIds.Add(deviceId);
                }
            }

            return user;
        }

        public bool CanAccessDevice(Guid deviceId) => allowedDeviceIds.Contains(deviceId);

        public void AllowDevice(Guid deviceId)
        {
            if (deviceId == Guid.Empty)
                throw new ArgumentException("Device ID cannot be empty.", nameof(deviceId));

            allowedDeviceIds.Add(deviceId);
        }
        public void RevokeDevice(Guid deviceId) { allowedDeviceIds.Remove(deviceId); } 
    }
}
