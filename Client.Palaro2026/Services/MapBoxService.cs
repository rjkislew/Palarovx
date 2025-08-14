using Microsoft.JSInterop;

public class MapBoxService
{
    private static Action<int>? onVenueSelected;
    private readonly IConfiguration _config;

    public MapBoxService(IConfiguration config)
    {
        _config = config;
        MapboxToken = _config["Mapbox:AccessToken"] ?? "";
    }

    public string MapboxToken { get; }
    public static Action<int>? OnVenueSelected { get => onVenueSelected; set => onVenueSelected = value; }

    [JSInvokable("SetVenueId")]
    public static Task SetVenueId(int id)
    {
        OnVenueSelected?.Invoke(id);
        return Task.CompletedTask;
    }
}
