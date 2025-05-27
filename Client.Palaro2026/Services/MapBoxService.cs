using Microsoft.JSInterop;

public class MapBoxService
{
    public static Action<int>? OnVenueSelected;


    [JSInvokable("SetVenueId")]
    public static Task SetVenueId(int id)
    {
        OnVenueSelected?.Invoke(id);
        return Task.CompletedTask;
    }


    public string mapboxToken { get; set; } = "pk.eyJ1IjoicGdhc3dlYm1hc3RlciIsImEiOiJjbTd0dTQzbnYwc3J0Mm1xeW03OHZvcTJhIn0.zNe2lZ__NbPkmFYRo_Y5aw";
}
