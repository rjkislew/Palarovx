// Declare sportsComplex and initialize user location variables
const sportsComplex = [125.94130366163259, 8.55111787984696];
let userLat = null;
let userLong = null;
let userLocation = sportsComplex;  // Default to sportsComplex initially

// Initialize map and geolocation handling
window.initializeMap = (containerId, token) => {
    mapboxgl.accessToken = token;

    window.map = new mapboxgl.Map({
        container: containerId,
        style: 'mapbox://styles/mapbox/outdoors-v12',
        center: userLocation, // Default center
        zoom: 17,
        pitch: 60,
        textColor: "#ffffff",
        antialias: true
    });

    window.map.on('style.load', () => {
        window.map.addSource('mapbox-dem', {
            type: 'raster-dem',
            url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
            tileSize: 512,
            maxzoom: 14
        });
        window.map.setTerrain({ source: 'mapbox-dem', exaggeration: 0 });
    });

    // Add GeolocateControl to the map
    const geolocateControl = new mapboxgl.GeolocateControl({
        positionOptions: {
            enableHighAccuracy: true
        },
        trackUserLocation: true,
        showUserHeading: true,
        showAccuracyCircle: true,
        showUserLocation: true
    });

    window.map.addControl(geolocateControl).addControl(new mapboxgl.NavigationControl());

    // Listen for the geolocate event to update user coordinates
    geolocateControl.on('geolocate', (event) => {
        userLat = event.coords.latitude;
        userLong = event.coords.longitude;
        userLocation = [userLong, userLat];

        // Set map center to user location
        window.map.setCenter(userLocation);
    });

    window.map.on('style.load', () => {
        // Insert the layer beneath any symbol layer.
        const layers = window.map.getStyle().layers;
        const labelLayer = layers.find(
            (layer) => layer.type === 'symbol' && layer.layout['text-field']
        ).id;

        // Add 3D buildings layer
        window.map.addLayer(
            {
                'id': 'add-3d-buildings',
                'source': 'composite',
                'source-layer': 'building',
                'filter': ['==', 'extrude', 'true'],
                'type': 'fill-extrusion',
                'minzoom': 15,
                'paint': {
                    'fill-extrusion-color': '#aaa',
                    'fill-extrusion-height': [
                        'interpolate',
                        ['linear'],
                        ['zoom'],
                        15,
                        0,
                        15.05,
                        ['get', 'height']
                    ],
                    'fill-extrusion-base': [
                        'interpolate',
                        ['linear'],
                        ['zoom'],
                        15,
                        0,
                        15.05,
                        ['get', 'min_height']
                    ],
                    'fill-extrusion-opacity': 0.6
                }
            },
            labelLayer
        );
    });
};
