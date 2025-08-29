using System.Text.Json;
using System.Text;

namespace Client.Palaro2026.Services
{
    public class APIService(IConfiguration configuration, HttpClient httpClient)
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;

        public bool NoData = false;
        public string ApiUrl => _configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl is not configured.");
        public string Palaro2026API => $"{ApiUrl}api";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private string BuildUrl(string relativeUrl)
        {
            if (!relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return $"{Palaro2026API.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
            }
            return relativeUrl;
        }

        public async Task<List<T>?> GetAsync<T>(string relativeUrl)
        {
            string url = BuildUrl(relativeUrl);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
            }
            catch
            {
                NoData = true;
                return null;
            }
        }

        public async Task<T?> GetSingleAsync<T>(string relativeUrl)
        {
            string url = BuildUrl(relativeUrl);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                // Ensure the deserialized list is not null before calling FirstOrDefault
                var list = JsonSerializer.Deserialize<List<T>>(responseContent, _jsonOptions);
                return list != null ? list.FirstOrDefault() : default;
            }
            catch
            {
                NoData = true;
                return default;
            }
        }
    }
}