using Microsoft.JSInterop;

namespace Client.Palaro2026.Services
{
    public class MapBoxService
    {
        private static Action<int>? onVenueSelected;


        [JSInvokable("SetVenueId")]
        public static Task SetVenueId(int id)
        {
            OnVenueSelected?.Invoke(id);
            return Task.CompletedTask;
        }

        public string MapboxToken { get; set; } = "pk.eyJ1IjoicGdhc3dlYm1hc3RlciIsImEiOiJjbTd0dTQzbnYwc3J0Mm1xeW03OHZvcTJhIn0.zNe2lZ__NbPkmFYRo_Y5aw";
        public static Action<int>? OnVenueSelected { get => onVenueSelected; set => onVenueSelected = value; }
    }
}