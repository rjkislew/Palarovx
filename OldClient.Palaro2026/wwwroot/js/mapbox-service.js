// Declare sportsComplex and initialize user location variables
const sportsComplex = [125.940565259789, 8.55068128311271];
let userLat = null;
let userLong = null;
let userLocation = sportsComplex;  // Default to sportsComplex initially

// Initialize map and geolocation handling
window.initializeMap = (containerId, token, markers) => {
    mapboxgl.accessToken = token;

    // Get CSS variable values
    const color = getComputedStyle(document.documentElement)
        .getPropertyValue('color')
        .trim();

    // Initialize the map
    window.map = new mapboxgl.Map({
        container: containerId,
        style: 'mapbox://styles/mapbox/outdoors-v12',
        center: sportsComplex,
        zoom: 9,
        textColor: "#ffffff",
        antialias: true
    });

    // Convert markers to GeoJSON format
    const geojson = {
        type: 'FeatureCollection',
        features: markers.map(location => ({
            type: 'Feature',
            geometry: {
                type: 'Point',
                coordinates: [location.longitude, location.latitude]
            },
            properties: {
                venue: location.venue || location.billetingQuarter,
                latitude: location.latitude,
                longitude: location.longitude
            }
        }))
    };

    window.map.on('load', () => {

        // Insert the layer beneath any symbol layer.
        const layers = map.getStyle().layers;
        const labelLayerId = layers.find(
            (layer) => layer.type === 'symbol' && layer.layout['text-field']
        ).id;

        // The 'building' layer in the Mapbox Streets
        // vector tileset contains building height data
        // from OpenStreetMap.
        map.addLayer(
            {
                'id': 'add-3d-buildings',
                'source': 'composite',
                'source-layer': 'building',
                'filter': ['==', 'extrude', 'true'],
                'type': 'fill-extrusion',
                'minzoom': 15,
                'paint': {
                    'fill-extrusion-color': '#aaa',

                    // Use an 'interpolate' expression to
                    // add a smooth transition effect to
                    // the buildings as the user zooms in.
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
                    'fill-extrusion-opacity': 1
                }
            },
            labelLayerId
        );

        // Add source with clustering enabled
        window.map.addSource('markers', {
            type: 'geojson',
            data: geojson,
            cluster: true,
            clusterMaxZoom: 14, // Max zoom level to cluster points
            clusterRadius: 50    // Radius of each cluster when clustering points
        });

        // Add clustered layer
        window.map.addLayer({
            id: 'clusters',
            type: 'circle',
            source: 'markers',
            filter: ['has', 'point_count'],
            paint: {
                'circle-color': [
                    'step',
                    ['get', 'point_count'],
                    color, // color for small clusters
                    10, color, // color for medium clusters
                    30, color  // color for large clusters
                ],
                'circle-radius': [
                    'step',
                    ['get', 'point_count'],
                    15,
                    10, 20,
                    30, 25
                ]
            }
        });

        // Add a layer for cluster labels
        window.map.addLayer({
            id: 'cluster-count',
            type: 'symbol',
            source: 'markers',
            filter: ['has', 'point_count'],
            layout: {
                'text-field': '{point_count_abbreviated}',
                'text-font': ['DIN Offc Pro Medium', 'Arial Unicode MS Bold'],
                'text-size': 12,
                'color': '#fff'
            }
        });

        // Add layer for individual markers outside clusters
        window.map.addLayer({
            id: 'unclustered-point',
            type: 'circle',
            source: 'markers',
            filter: ['!', ['has', 'point_count']],
            paint: {
                'circle-color': color,
                'circle-radius': 8,
                'circle-stroke-width': 1,
                'circle-stroke-color': '#fff'
            }
        });

        // Popup for individual markers
        window.map.on('click', 'unclustered-point', (e) => {
            const coordinates = e.features[0].geometry.coordinates.slice();
            const { venue, latitude, longitude } = e.features[0].properties;

            const popupContent = `
                <div class="popup-content">
                    <h3>${venue}</h3>
                    <button class="get-directions-button" onclick="getLocation(${latitude}, ${longitude})">Get Directions</button>
                </div>
            `;

            new mapboxgl.Popup()
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(window.map);
        });

        // Handle cluster clicks to zoom in
        window.map.on('click', 'clusters', (e) => {
            const features = window.map.queryRenderedFeatures(e.point, { layers: ['clusters'] });
            const clusterId = features[0].properties.cluster_id;

            window.map.getSource('markers').getClusterExpansionZoom(clusterId, (err, zoom) => {
                if (err) return;

                window.map.easeTo({
                    center: features[0].geometry.coordinates,
                    zoom: zoom
                });
            });
        });
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

    geolocateControl.on('geolocate', (event) => {
        userLat = event.coords.latitude;
        userLong = event.coords.longitude;
        userLocation = [userLong, userLat];

        console.log("User location updated:", userLocation);

        if (window.selectedDestination) {
            showDirections(userLocation, window.selectedDestination);
        }
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
    const origin = userLocation;

    // Create the Directions API URL
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
                zoom: 20,
                pitch: 60,
                bearing: -60,
                speed: 1.2,
                curve: 1.2,
                duration: 5000
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

// Function triggered when the "Get Directions" button is clicked
window.getLocation = (latitude, longitude) => {
    const destination = [longitude, latitude];
    window.selectedDestination = destination; // Store the selected destination
    window.showDirections(destination); // Call the showDirections function
};