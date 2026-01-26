using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MudBlazor.Colors;


public class APIService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    //sr
    private readonly IWebAssemblyHostEnvironment _environment;

    public APIService(IConfiguration configuration, HttpClient httpClient, IWebAssemblyHostEnvironment environment)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        //sr
        _environment = environment;
    }

    public string ApiUrl => _configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl is not configured.");
    public string Palaro2026API => $"{ApiUrl}api";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string BuildUrl(string relativeUrl)
    {
        //sir ronald
        if (_environment.IsDevelopment())
        {
            return $"https://localhost:7063/api/{relativeUrl.TrimStart('/')}";
        }
        //
        if (!relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return $"{Palaro2026API.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
        return relativeUrl;
    }

    public async Task<HttpResponseMessage> PostRawAsync<TRequest>(string relativeUrl, TRequest data)
    {
        string url = BuildUrl(relativeUrl);
        return await _httpClient.PostAsJsonAsync(url, data);
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

    public async Task<bool> PatchAsync<T>(string relativeUrl, T data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var options = new JsonSerializerOptions
            {
                // Allow nulls to be included in JSON
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(data, options),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PatchAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }


    public async Task<TResponse?> PatchAndReadAsync<TRequest, TResponse>(string relativeUrl, TRequest data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
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

    public async Task<bool> PutFormAsync(string relativeUrl, MultipartFormDataContent formData)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var response = await _httpClient.PutAsync(url, formData);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T?> PostFormAsync<T>(string url, MultipartFormDataContent content)
    {
        var requestUrl = BuildUrl(url);
        var response = await _httpClient.PostAsync(requestUrl, content);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
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

    public async Task<TResult?> PostAsync<TRequest, TResult>(string relativeUrl, TRequest data)
    {
        string url = BuildUrl(relativeUrl);
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                // Try to parse as the expected type
                try
                {
                    return JsonSerializer.Deserialize<TResult>(responseContent, _jsonOptions);
                }
                catch
                {
                    // If direct deserialization fails, try to handle different response formats
                    if (typeof(TResult) == typeof(int))
                    {
                        if (int.TryParse(responseContent, out int intResult))
                        {
                            return (TResult)(object)intResult;
                        }
                    }
                    return default;
                }
            }
            else
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");
                return default;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Call Error: {ex.Message}");
            return default;
        }
    }
}
