using System.Text.Json;
using System.Text;


public class APIService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public APIService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

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
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> PostAsync<T>(string relativeUrl, T data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<T?> PostAndReadAsync<T>(string relativeUrl, object data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }


    public async Task<bool> PutAsync<T>(string relativeUrl, T data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string relativeUrl)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
