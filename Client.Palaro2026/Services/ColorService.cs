using Blazored.LocalStorage;

public class ColorService
{
    private readonly ILocalStorageService _localStorage;

    public string BackgroundColor { get; set; } = "#1e4ca1";
    public string BlueColor { get; set; } = "#1e4ca1";
    public string YellowColor { get; set; } = "#ebb94d";
    public string RedColor { get; set; } = "#ba3535";

    public bool ToggledBlue { get; set; }
    public bool ToggledYellow { get; set; }
    public bool ToggledRed { get; set; }

    public bool DisableBlue { get; set; }
    public bool DisableYellow { get; set; }
    public bool DisableRed { get; set; }

    public event Action OnChange;

    public ColorService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();

        // If no color is toggled after loading settings, set the color based on the BackgroundColor
        if (!ToggledBlue && !ToggledYellow && !ToggledRed)
        {
            if (BackgroundColor == BlueColor)
            {
                ToggledBlue = true;
                DisableBlue = true;
                DisableYellow = DisableRed = false;
            }
            else if (BackgroundColor == YellowColor)
            {
                ToggledYellow = true;
                DisableYellow = true;
                DisableBlue = DisableRed = false;
            }
            else if (BackgroundColor == RedColor)
            {
                ToggledRed = true;
                DisableRed = true;
                DisableBlue = DisableYellow = false;
            }
            else
            {
                // If BackgroundColor doesn't match any color, set blue as default
                ToggledBlue = true;
                DisableBlue = true;
                DisableYellow = DisableRed = false;
                BackgroundColor = BlueColor;
            }

            await SaveSettingsAsync();
        }

        NotifyStateChanged();
    }

    public async Task LoadSettingsAsync()
    {
        var storedBackgroundColor = await _localStorage.GetItemAsStringAsync("BackgroundColor");
        var storedBlueColor = await _localStorage.GetItemAsStringAsync("BlueColor");
        var storedYellowColor = await _localStorage.GetItemAsStringAsync("YellowColor");
        var storedRedColor = await _localStorage.GetItemAsStringAsync("RedColor");

        // Clean the stored values by removing extra quotes and escape characters
        BackgroundColor = storedBackgroundColor != null ? CleanStoredValue(storedBackgroundColor) : BackgroundColor;
        BlueColor = storedBlueColor != null ? CleanStoredValue(storedBlueColor) : BlueColor;
        YellowColor = storedYellowColor != null ? CleanStoredValue(storedYellowColor) : YellowColor;
        RedColor = storedRedColor != null ? CleanStoredValue(storedRedColor) : RedColor;


        ToggledBlue = bool.TryParse(await _localStorage.GetItemAsStringAsync("ToggledBlue"), out var blue) && blue;
        ToggledYellow = bool.TryParse(await _localStorage.GetItemAsStringAsync("ToggledYellow"), out var yellow) && yellow;
        ToggledRed = bool.TryParse(await _localStorage.GetItemAsStringAsync("ToggledRed"), out var red) && red;

        DisableBlue = bool.TryParse(await _localStorage.GetItemAsStringAsync("DisableBlue"), out var disableBlue) && disableBlue;
        DisableYellow = bool.TryParse(await _localStorage.GetItemAsStringAsync("DisableYellow"), out var disableYellow) && disableYellow;
        DisableRed = bool.TryParse(await _localStorage.GetItemAsStringAsync("DisableRed"), out var disableRed) && disableRed;

        NotifyStateChanged();
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
        await _localStorage.SetItemAsync("BackgroundColor", BackgroundColor);
        await _localStorage.SetItemAsync("BlueColor", BlueColor);
        await _localStorage.SetItemAsync("YellowColor", YellowColor);
        await _localStorage.SetItemAsync("RedColor", RedColor);

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
                    BackgroundColor = BlueColor;
                }
                else
                {
                    // Enable all colors
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
                    BackgroundColor = YellowColor;
                }
                else
                {
                    // Enable all colors
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
                    BackgroundColor = RedColor;
                }
                else
                {
                    // Enable all colors
                    DisableBlue = DisableYellow = DisableRed = false;
                }
                break;
        }

        SaveSettingsAsync().ConfigureAwait(false);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}