using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    public event Action? OnThemeChanged;
    public bool? IsDarkMode { get; private set; } = null;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task LoadThemePreference()
    {
        var storedValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "darkMode");
        if (!string.IsNullOrEmpty(storedValue))
        {
            IsDarkMode = storedValue switch
            {
                "true" => true,
                "false" => false,
                _ => null
            };
            NotifyStateChanged();
        }
    }

    public async Task SaveThemePreference(bool? isDarkMode)
    {
        IsDarkMode = isDarkMode;
        var value = isDarkMode?.ToString().ToLower() ?? "null";
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "darkMode", value);
        NotifyStateChanged();
    }

    public void ToggleDarkMode()
    {
        IsDarkMode = IsDarkMode switch
        {
            null => true,
            true => false,
            false => null
        };
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnThemeChanged?.Invoke();
}
