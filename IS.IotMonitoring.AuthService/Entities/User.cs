namespace IS.IotMonitoring.AuthService.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRole Role { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private User() { }

        public static User Create(string username, string password, UserRole role)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            return new User
            {
                Id = Guid.NewGuid(),
                Username = NormalizeUsername(username),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public static string NormalizeUsername(string username) => username.Trim().ToLowerInvariant();

        public bool VerifyPassword(string password) => BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}
