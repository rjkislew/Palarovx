// Declare sportsComplex and initialize user location variables
const sportsComplex = [125.940565259789, 8.55068128311271];
let userLat = null;
let userLong = null;
let userLocation = sportsComplex;  // Default to sportsComplex initially

// Initialize map and geolocation handling
window.initializeMap = (containerId, token, markers) => {
    mapboxgl.accessToken = token;

    // Initialize the map
    window.map = new mapboxgl.Map({
        container: containerId,
        style: 'mapbox://styles/mapbox/outdoors-v12',
        center: sportsComplex, // Keep center as the sportsComplex
        zoom: 9,
        textColor: "#ffffff",
        antialias: true
    });

    markers.forEach(location => {
        new mapboxgl.Marker()
            .setLngLat([location.longitude, location.latitude])
            .setPopup(new mapboxgl.Popup({ offset: 25 })
                .setHTML(`<h3>${location.location || location.school_name}</h3>`))
            .addTo(window.map);
    });

    // Handle user geolocation
    const geolocateControl = new mapboxgl.GeolocateControl({
        positionOptions: { enableHighAccuracy: true },
        trackUserLocation: true,
        showUserHeading: true,
        showAccuracyCircle: true,
        showUserLocation: true
    });

    window.map.addControl(geolocateControl).addControl(new mapboxgl.NavigationControl());

    // Update user location on geolocate event
    geolocateControl.on('geolocate', (event) => {
        userLat = event.coords.latitude;
        userLong = event.coords.longitude;
        userLocation = [userLong, userLat];

        // If a destination is selected, update the route
        if (window.selectedDestination) {
            showDirections(userLocation, window.selectedDestination);
        }
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

// Function to show directions
window.showDirections = (destination) => {
    // Ensure that destination is valid
    if (!destination || destination.length !== 2) {
        console.error("Invalid destination provided to showDirections.");
        return;
    }

    // Assuming userLocation is already set from geolocation
    const origin = userLocation;  // You can use your user location or a fixed origin if needed

    // Create the Directions API URL using the correct format
    const directionsUrl = `https://api.mapbox.com/directions/v5/mapbox/driving/${origin[0]},${origin[1]};${destination[0]},${destination[1]}?alternatives=true&geometries=geojson&language=en&overview=full&steps=true&access_token=${mapboxgl.accessToken}`;

    // Clear previous route if it exists
    if (window.map.getSource('route')) {
        window.map.removeLayer('route');
        window.map.removeSource('route');
    }

    // Fetch directions
    fetch(directionsUrl)
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            // Check if the route data is valid
            if (!data.routes || data.routes.length === 0) {
                console.error("No routes found in the response.");
                return;
            }
            const route = data.routes[0].geometry.coordinates;

            // Create a GeoJSON source and add to the map
            window.map.addSource('route', {
                'type': 'geojson',
                'data': {
                    'type': 'Feature',
                    'properties': {},
                    'geometry': {
                        'type': 'LineString',
                        'coordinates': route
                    }
                }
            });

            // Add a line layer for the route
            window.map.addLayer({
                'id': 'route',
                'type': 'line',
                'source': 'route',
                'layout': {
                    'line-join': 'round',
                    'line-cap': 'round'
                },
                'paint': {
                    'line-color': '#FF6D00',
                    'line-width': 8
                }
            });

            // Fly to the destination
            window.map.flyTo({
                center: destination,
                essential: true, // This animation is considered essential with respect to prefers-reduced-motion
                zoom: 20, // Adjust the zoom level as needed
                pitch: 60, // pitch in degrees
                bearing: -60, // bearing in degrees
                speed: 1.2, // The speed of the animation
                curve: 1.2, // The easing of the animation
                duration: 5000 // Duration of the animation in milliseconds
            });
        })
        .catch(error => console.error('Error fetching directions:', error));
};

window.centerMap = (userLocation, destination) => {
    // Determine the center location: user location or default sports complex location
    const centerLocation = userLocation || [125.94130366163259, 8.55111787984696]; // Default to sports complex

    // Reset the bearing and pitch to default values
    window.map.setBearing(0); // Reset bearing to 0 (north)
    window.map.setPitch(0); // Reset pitch to 0 (straight down)
    window.map.setZoom(9); // Set the zoom level to 9
    window.map.setCenter(centerLocation); // Center the map to the determined center location and set the zoom level
};

// Function to clear the route from the map
window.clearRoute = () => {
    // Remove the route layer and source if they exist
    if (window.map.getLayer('route')) {
        window.map.removeLayer('route');
    }
    if (window.map.getSource('route')) {
        window.map.removeSource('route');
    }
};

