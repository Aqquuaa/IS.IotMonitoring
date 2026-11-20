using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IS.IotMonitoring.DataGenerator.Options;
using Microsoft.Extensions.Options;

namespace IS.IotMonitoring.DataGenerator.Api
{
    public class IotApiClient
    {
        public const string HttpClientName = "iot";
        private const string TemperatureUnit = "°C";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly GeneratorOptions options;
        private string? accessToken;
        private DateTimeOffset tokenExpiresAt;

        public IotApiClient(IHttpClientFactory httpClientFactory, IOptions<GeneratorOptions> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.options = options.Value;
        }

        public async Task<IReadOnlyList<DeviceDto>> GetActiveTemperatureSensorsAsync(CancellationToken cancellationToken)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, "api/devices", null, cancellationToken);
            using var response = await SendAsync(request, cancellationToken);

            var devices = await response.Content.ReadFromJsonAsync<List<DeviceDto>>(cancellationToken) ?? [];
            return devices
                .Where(d => d.Type == DeviceType.TemperatureSensor && d.Status != DeviceStatus.Deleted)
                .ToList();
        }

        public async Task<DeviceDto> CreateTemperatureSensorAsync(string name, CancellationToken cancellationToken)
        {
            var body = new CreateDeviceRequest(name, DeviceType.TemperatureSensor);
            using var request = await CreateRequestAsync(HttpMethod.Post, "api/devices", body, cancellationToken);
            using var response = await SendAsync(request, cancellationToken);

            return await response.Content.ReadFromJsonAsync<DeviceDto>(cancellationToken)
                ?? throw new InvalidOperationException("CoreApi returned an empty device.");
        }

        public async Task SendReadingAsync(Guid deviceId, double reading, DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            var body = new RecordTelemetryRequest(deviceId, reading, TemperatureUnit, timestamp);
            using var request = await CreateRequestAsync(HttpMethod.Post, "api/telemetry", body, cancellationToken);
            using var response = await SendAsync(request, cancellationToken);
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
        {
            await EnsureTokenAsync(cancellationToken);

            var request = new HttpRequestMessage(method, new Uri(new Uri(options.CoreApiUrl), path));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (body is not null)
                request.Content = JsonContent.Create(body);

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                accessToken = null;

            response.EnsureSuccessStatusCode();
            return response;
        }

        private async Task EnsureTokenAsync(CancellationToken cancellationToken)
        {
            if (accessToken is not null && DateTimeOffset.UtcNow < tokenExpiresAt.AddMinutes(-1))
                return;

            var loginUrl = new Uri(new Uri(options.AuthServiceUrl), "api/auth/login");
            using var response = await httpClientFactory.CreateClient(HttpClientName)
                .PostAsJsonAsync(loginUrl, new LoginRequest(options.Username, options.Password), cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"Login as '{options.Username}' failed. Check Generator:Username and Generator:Password.");

            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new InvalidOperationException("AuthService returned an empty token.");

            accessToken = token.AccessToken;
            tokenExpiresAt = token.ExpiresAt;
        }
    }
}
