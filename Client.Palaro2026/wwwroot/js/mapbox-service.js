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
                address: location.address,
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

        // Define custom colors for clusters
        const clusterColors = ['#4CAF50', '#FF9800', '#F44336']; // Green, Orange, Red

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
                    clusterColors[0], // Green for small clusters
                    10, clusterColors[1], // Orange for medium clusters
                    30, clusterColors[2]  // Red for large clusters
                ],
                'circle-radius': [
                    'step',
                    ['get', 'point_count'],
                    15,  // Small clusters
                    10, 20,  // Medium clusters
                    30, 25   // Large clusters
                ],
                'circle-stroke-width': 2,
                'circle-stroke-color': '#fff'
            }
        });

        // Add a layer for cluster labels (show count on clusters)
        window.map.addLayer({
            id: 'cluster-count',
            type: 'symbol',
            source: 'markers',
            filter: ['has', 'point_count'],
            layout: {
                'text-field': '{point_count_abbreviated}',
                'text-font': ['DIN Offc Pro Medium', 'Arial Unicode MS Bold'],
                'text-size': 14
            },
            paint: {
                'text-color': '#fff' // White text on clusters
            }
        });

        // Add layer for individual markers outside clusters
        window.map.addLayer({
            id: 'unclustered-point',
            type: 'circle',
            source: 'markers',
            filter: ['!', ['has', 'point_count']],
            paint: {
                'circle-color': '#007AFF', // Blue for unclustered markers
                'circle-radius': 8,
                'circle-stroke-width': 2,
                'circle-stroke-color': '#fff'
            }
        });

        // Add venue name labels for unclustered markers
        window.map.addLayer({
            id: 'unclustered-label',
            type: 'symbol',
            source: 'markers',
            filter: ['!', ['has', 'point_count']],
            layout: {
                'text-field': ['get', 'venue'],
                'text-font': ['DIN Offc Pro Medium', 'Arial Unicode MS Bold'],
                'text-size': 12,
                'text-anchor': 'top'
            },
            paint: {
                'text-color': '#333', // Dark text
                'text-halo-color': '#fff', // White halo for readability
                'text-halo-width': 2
            }
        });


        // Popup for individual markers
        window.map.on('click', 'unclustered-point', (e) => {
            const coordinates = e.features[0].geometry.coordinates.slice();
            const { id, venue, address, latitude, longitude } = e.features[0].properties;

            const popupContent = `
                <div class="popup-content" style="width: auto;">
                    <h3>${venue}</h3>
                    <h4>${address}</h4>
                    <button style="margin-top: 5px" class="get-directions-button" onclick="getLocation(${latitude}, ${longitude})">Get Directions</button>
                </div>
            `;

            new mapboxgl.Popup()
                .setLngLat(coordinates)
                .setHTML(popupContent)
                .addTo(window.map);
        });

        let clusterPopup = null; // Track the cluster popup

        // Handle cluster clicks to show venue names as a clickable list
        window.map.on('click', 'clusters', async (e) => {
            const features = window.map.queryRenderedFeatures(e.point, { layers: ['clusters'] });
            if (!features.length) return;

            const clusterId = features[0].properties.cluster_id;

            // Get the cluster expansion zoom
            window.map.getSource('markers').getClusterExpansionZoom(clusterId, (err, zoom) => {
                if (err) return;

                window.map.easeTo({
                    center: features[0].geometry.coordinates,
                    zoom: zoom
                });
            });

            // Fetch cluster's children (list of markers inside the cluster)
            window.map.getSource('markers').getClusterLeaves(clusterId, 50, 0, (err, leaves) => {
                if (err) {
                    console.error('Error fetching cluster leaves:', err);
                    return;
                }

                // Extract venue details
                const venues = leaves.map(leaf => ({
                    name: leaf.properties.venue,
                    address: leaf.properties.address,
                    latitude: leaf.properties.latitude,
                    longitude: leaf.properties.longitude,
                    coordinates: leaf.geometry.coordinates
                })).filter(venue => venue.name); // Filter out any null names

                // Create a popup with venue names as clickable items
                if (venues.length > 0) {
                    const popupContent = `
                        <div style="max-height: auto; overflow-y: auto;">
                            <h3>Venues in this area:</h3>
                            <ul style="list-style: none;">
                                ${venues.map(venue => `
                                    <li>
                                        <button 
                                            style="border: none; background: none; color: blue; cursor: pointer;"
                                            onclick="zoomToVenue(${venue.longitude}, ${venue.latitude}, '${venue.name}', '${venue.address}', true)">
                                            ${venue.name}
                                        </button>
                                    </li>
                                `).join('')}
                            </ul>
                        </div>
                    `;

                    // Remove any existing cluster popup before creating a new one
                    if (clusterPopup) {
                        clusterPopup.remove();
                    }

                    clusterPopup = new mapboxgl.Popup()
                        .setLngLat(features[0].geometry.coordinates)
                        .setHTML(popupContent)
                        .addTo(window.map);
                }
            });
        });

        // Function to zoom to a venue and show a popup
        window.zoomToVenue = (lng, lat, venueName, address = "Address not available", closeClusterPopup = false) => {
            // Close cluster popup when selecting a venue
            if (closeClusterPopup && clusterPopup) {
                clusterPopup.remove();
                clusterPopup = null;
            }

            // Close existing individual popups
            if (window.currentPopup) {
                window.currentPopup.remove();
            }

            window.map.flyTo({
                center: [lng, lat],
                zoom: 15,
                essential: true
            });

            const popupContent = `
                <div class="popup-content" style="width: auto;">
                    <h3>${venueName}</h3>
                    <h4>${address}</h4>
                    <button style="margin-top: 5px" onclick="sendVenueToBlazor('${venueName}')">
                        Get Directions
                    </button>
                </div>
            `;


            window.currentPopup = new mapboxgl.Popup()
                .setLngLat([lng, lat])
                .setHTML(popupContent)
                .addTo(window.map);
        };

    });

    // Handle user geolocation
    const geolocateControl = new mapboxgl.GeolocateControl({
        positionOptions: { enableHighAccuracy: true },
        trackUserLocation: true,
        showUserHeading: true,
        showAccuracyCircle: true,
        showUserLocation: true
    });

    window.map.addControl(geolocateControl, 'bottom-right').addControl(new mapboxgl.NavigationControl(), 'bottom-right');

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

window.registerBlazorObject = function (blazorRef) {
    window.blazorComponent = blazorRef;
};

window.sendVenueToBlazor = function (venueName) {
    if (window.blazorComponent) {
        window.blazorComponent.invokeMethodAsync('SetSelectedVenueFromJS', venueName)
            .then(() => console.log("Venue sent to Blazor:", venueName))
            .catch(err => console.error("Blazor call failed:", err));
    } else {
        console.error("Blazor reference not set.");
    }
};
