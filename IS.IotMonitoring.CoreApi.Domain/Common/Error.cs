namespace IS.IotMonitoring.CoreApi.Domain.Common
{
    public record Error(string Id, string Message)
    {
        public static Error None = new(string.Empty, string.Empty);
        public static Error NotFound(string message) => new("NotFound", message);
        public static Error Conflict(string message) => new("Conflict", message);
    };
}
