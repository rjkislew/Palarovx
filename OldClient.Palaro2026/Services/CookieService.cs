using Microsoft.JSInterop;

public class CookieService
{
    private readonly IJSRuntime _jsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? OnChange;

    public async Task<string?> GetCookie(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("cookieService.getCookie", key);
    }

    public async Task SetCookie(string key, string value, int? expirationInSeconds = null, bool secure = true, string sameSite = "Strict")
    {
        var expiration = expirationInSeconds ?? 0; // Default to 0 if no expiration is provided

        // Pass all the flags to JavaScript
        await _jsRuntime.InvokeVoidAsync("cookieService.setCookie", key, value, expiration, secure, sameSite);

        NotifyCookieChanged();
    }



    public async Task ClearCookie(string key)
    {
        // Clear the cookie in JS
        await _jsRuntime.InvokeVoidAsync("cookieService.deleteCookie", key);

        // Notify listeners of the change
        NotifyCookieChanged();
    }

    private void NotifyCookieChanged() => OnChange?.Invoke();
}
