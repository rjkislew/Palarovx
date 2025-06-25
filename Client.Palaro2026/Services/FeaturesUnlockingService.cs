using Microsoft.JSInterop;
using static MudBlazor.Colors;

namespace Client.Palaro2026.Services
{
    public class FeaturesUnlockingService(IJSRuntime jsRuntime)
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public event Action? OnLockChanged;

        public bool IsUnlocked { get; private set; }

        public async Task ToggleUnlockFeaturesAsync()
        {
            IsUnlocked = !IsUnlocked;

            var value = IsUnlocked.ToString().ToLower(); // "true" or "false"
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isUnlocked", value);

            NotifyStateChanged();
        }
        public async Task UnlockFeaturesAsync()
        {
            IsUnlocked = true;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isUnlocked", IsUnlocked.ToString().ToLower());
        }


        public async Task LoadFromLocalStorageAsync()
        {
            var isUnlockedString = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "isUnlocked");

            if (!string.IsNullOrEmpty(isUnlockedString) && isUnlockedString != "null")
            {
                var previous = IsUnlocked;
                IsUnlocked = isUnlockedString switch
                {
                    "true" => true,
                    "false" => false,
                    _ => false
                };

                if (IsUnlocked != previous)
                {
                    OnLockChanged?.Invoke();
                }
            }
        }


        private void NotifyStateChanged() => OnLockChanged?.Invoke();
    }
}