using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Client.Palaro2026.Services
{
    public class ThemeService(IJSRuntime jsRuntime)
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public event Action? OnThemeChanged;

        public bool? IsDarkMode { get; private set; } = null;
        public bool UserSelectedTheme { get; private set; } = false;

        public async Task LoadThemePreference()
        {
            var storedValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "darkMode");

            if (!string.IsNullOrEmpty(storedValue) && storedValue != "null")
            {
                IsDarkMode = storedValue switch
                {
                    "true" => true,
                    "false" => false,
                    _ => null
                };
                UserSelectedTheme = true;
            }
            else
            {
                IsDarkMode = null;
                UserSelectedTheme = false;
            }

            NotifyStateChanged();
        }

        public async Task ToggleDarkModeAsync()
        {
            IsDarkMode = IsDarkMode switch
            {
                null => true,   // Auto → Dark
                true => false,  // Dark → Light
                false => null   // Light → Auto
            };

            UserSelectedTheme = IsDarkMode != null;

            var value = IsDarkMode?.ToString().ToLower() ?? "null";
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "darkMode", value);

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnThemeChanged?.Invoke();
    }
}