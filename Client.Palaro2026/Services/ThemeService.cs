using Microsoft.JSInterop;

namespace Client.Palaro2026.Services
{
    public class ThemeService(IJSRuntime jsRuntime)
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public event Action? OnThemeChanged;

        public bool IsDarkMode { get; set; }

        public async Task LoadThemePreference()
        {
            var storedValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "isDarkMode");

            if (!string.IsNullOrEmpty(storedValue) && storedValue != "null")
            {
                IsDarkMode = storedValue switch
                {
                    "true" => true,
                    "false" => false,
                    _ => throw new NotImplementedException()
                };
            }

            NotifyStateChanged();
        }

        public async Task SystemThemeModeAsync(bool value)
        {
            IsDarkMode = value;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isDarkMode", value);

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnThemeChanged?.Invoke();
    }
}