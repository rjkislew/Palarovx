using Blazored.LocalStorage;

public class ColorService
{
    private readonly ILocalStorageService _localStorage;

    public string SelectedColor { get; set; } = "#1e4ca1";
    public string BlueColor { get; set; } = "#1e4ca1";
    public string YellowColor { get; set; } = "#e7a53c";
    public string RedColor { get; set; } = "#ba3535";

    public bool ToggledBlue { get; set; }
    public bool ToggledYellow { get; set; }
    public bool ToggledRed { get; set; }

    public bool DisableBlue { get; set; }
    public bool DisableYellow { get; set; }
    public bool DisableRed { get; set; }


    public event Action OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public ColorService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();

        // If no color is toggled after loading settings, set the color based on the SelectedColor
        if (!ToggledBlue && !ToggledYellow && !ToggledRed)
        {
            SetInitialColorState();
        }

        NotifyStateChanged();
    }

    private void SetInitialColorState()
    {
        if (SelectedColor == BlueColor)
        {
            ToggledBlue = true;
            DisableBlue = true;
            DisableYellow = DisableRed = false;
        }
        else if (SelectedColor == YellowColor)
        {
            ToggledYellow = true;
            DisableYellow = true;
            DisableBlue = DisableRed = false;
        }
        else if (SelectedColor == RedColor)
        {
            ToggledRed = true;
            DisableRed = true;
            DisableBlue = DisableYellow = false;
        }
        else
        {
            // If SelectedColor doesn't match any color, set blue as default
            ToggledBlue = true;
            DisableBlue = true;
            DisableYellow = DisableRed = false;
            SelectedColor = BlueColor;
        }
    }

    public async Task LoadSettingsAsync()
    {
        // Load and clean stored colors
        SelectedColor = await LoadAndCleanColorAsync("SelectedColor", SelectedColor);

        // Load toggled states
        ToggledBlue = await LoadBooleanAsync("ToggledBlue");
        ToggledYellow = await LoadBooleanAsync("ToggledYellow");
        ToggledRed = await LoadBooleanAsync("ToggledRed");

        // Load disable states
        DisableBlue = await LoadBooleanAsync("DisableBlue");
        DisableYellow = await LoadBooleanAsync("DisableYellow");
        DisableRed = await LoadBooleanAsync("DisableRed");

        // Ensure correct state is set based on SelectedColor
        SetInitialColorState();

        NotifyStateChanged();
    }

    private async Task<string> LoadAndCleanColorAsync(string key, string defaultValue)
    {
        var storedValue = await _localStorage.GetItemAsStringAsync(key);
        return storedValue != null ? CleanStoredValue(storedValue) : defaultValue;
    }

    private async Task<bool> LoadBooleanAsync(string key)
    {
        return bool.TryParse(await _localStorage.GetItemAsStringAsync(key), out var result) && result;
    }

    private string CleanStoredValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        // Remove escape characters and extra quotes
        return value.Replace("\\u0022", "").Replace("\"", "");
    }

    public async Task SaveSettingsAsync()
    {
        // Save values without extra quotes
        await _localStorage.SetItemAsync("SelectedColor", SelectedColor);

        await _localStorage.SetItemAsync("ToggledBlue", ToggledBlue.ToString());
        await _localStorage.SetItemAsync("ToggledYellow", ToggledYellow.ToString());
        await _localStorage.SetItemAsync("ToggledRed", ToggledRed.ToString());

        await _localStorage.SetItemAsync("DisableBlue", DisableBlue.ToString());
        await _localStorage.SetItemAsync("DisableYellow", DisableYellow.ToString());
        await _localStorage.SetItemAsync("DisableRed", DisableRed.ToString());
    }

    public void ToggleColor(string color)
    {
        switch (color)
        {
            case "Blue":
                ToggledBlue = !ToggledBlue;
                if (ToggledBlue)
                {
                    // Disable selected color, enable and uncheck others
                    DisableBlue = true;
                    DisableYellow = DisableRed = false;
                    ToggledYellow = ToggledRed = false;
                    SelectedColor = BlueColor;
                }
                else
                {
                    // Enable all colors if toggled off
                    DisableBlue = DisableYellow = DisableRed = false;
                }
                break;

            case "Yellow":
                ToggledYellow = !ToggledYellow;
                if (ToggledYellow)
                {
                    // Disable selected color, enable and uncheck others
                    DisableYellow = true;
                    DisableBlue = DisableRed = false;
                    ToggledBlue = ToggledRed = false;
                    SelectedColor = YellowColor;
                }
                else
                {
                    // Enable all colors if toggled off
                    DisableBlue = DisableYellow = DisableRed = false;
                }
                break;

            case "Red":
                ToggledRed = !ToggledRed;
                if (ToggledRed)
                {
                    // Disable selected color, enable and uncheck others
                    DisableRed = true;
                    DisableBlue = DisableYellow = false;
                    ToggledBlue = ToggledYellow = false;
                    SelectedColor = RedColor;
                }
                else
                {
                    // Enable all colors if toggled off
                    DisableBlue = DisableYellow = DisableRed = false;
                }
                break;
        }

        // Ensure settings are saved after toggling
        SaveSettingsAsync().ConfigureAwait(false); // Use ConfigureAwait(false) in async void methods
        NotifyStateChanged();
    }
}