using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly CookieService _cookieService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string API_URL = APIService.Palaro2026API;

    public AuthenticationService(HttpClient httpClient, CookieService cookieService, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _cookieService = cookieService;
        _logger = logger;
    }

    // Models for UserLoginDetails and LoginResponse
    public class UserLoginDetails
    {
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
    }
    public class UserID
    {
        public string? ID { get; set; } // Assuming ID is an integer; adjust type if necessary
    }

    public class LoginResponse
    {
        public UserID? UserID { get; set; } // UserID property is now of type UserID
        public string? Token { get; set; }
    }

    public async Task<LoginResponse?> Login(string username, string password)
    {
        var loginDetails = new UserLoginDetails
        {
            Username = username,
            PasswordHash = password,
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{API_URL}/Users/Login", loginDetails);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result?.Token != null)
                {
                    await _cookieService.SetCookie("SessionToken", result.Token, 86400);
                    await _cookieService.SetCookie("UserID", result.UserID?.ID, 86400);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                }

                return result;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Login failed: {errorMessage}");
                return null;
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError($"HTTP Request Error: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
            return null;
        }
    }

    public async Task Logout()
    {
        await _cookieService.ClearCookie("SessionToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<bool> IsLoggedIn()
    {
        var token = await _cookieService.GetCookie("SessionToken");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetToken()
    {
        return await _cookieService.GetCookie("SessionToken");
    }
}