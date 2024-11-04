using Blazored.LocalStorage;
using System.Net.Http.Json;

public class AuthenticationService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthenticationService(HttpClient http, ILocalStorageService localStorage, CustomAuthStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task Login(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsStringAsync();
        await _localStorage.SetItemAsync("token", token);
        await _authStateProvider.GetAuthenticationStateAsync();
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("token");
        await _authStateProvider.GetAuthenticationStateAsync();
    }
}