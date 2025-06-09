using Microsoft.JSInterop;

public class FeaturesUnlockingService
{
    private readonly IJSRuntime _jsRuntime;

    public event Action? OnLockChanged;

    public bool IsUnlocked { get; private set; }

    public FeaturesUnlockingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ToggleUnlockFeaturesAsync()
    {
        IsUnlocked = !IsUnlocked;

        var value = IsUnlocked.ToString().ToLower(); // "true" or "false"
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isUnlocked", value);

        NotifyStateChanged();
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
