// Declare defaultCoordinates globally
const defaultCoordinates = [125.94138286387499, 8.550991922751852];

// Initialize map and geolocation handling
window.initializeMap = (containerId, token, zoomLevel) => {
    mapboxgl.accessToken = token;

    window.map = new mapboxgl.Map({
        container: containerId,
        style: 'mapbox://styles/mapbox/outdoors-v12',
        center: defaultCoordinates, // Use default coordinates initially
        zoom: zoomLevel,
        pitch: 45,
        bearing: -17.6
    });

    // Add zoom and rotation controls to the map
    const nav = new mapboxgl.NavigationControl();
    window.map.addControl(nav, 'bottom-right');

    // Add geolocation control to the map
    const geoControl = new mapboxgl.GeolocateControl({
        positionOptions: {
            enableHighAccuracy: true
        },
        trackUserLocation: true, // Follow the user's location
        showUserHeading: true
    });
    window.map.addControl(geoControl, 'bottom-right');

    // Initialize markers array to avoid undefined issues
    window.markers = [];

    window.map.on('load', () => {
        console.log("Map has fully loaded.");
        geoControl.trigger(); // Automatically trigger geolocation on load
    });

    // Handle geolocation
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(successLocation, errorLocation, {
            enableHighAccuracy: true
        });
    } else {
        useDefaultLocation();
    }

    function successLocation(position) {
        const lng = position.coords.longitude;
        const lat = position.coords.latitude;

        window.map.setCenter([lng, lat]);
        window.map.setZoom(15);
        new mapboxgl.Marker().setLngLat([lng, lat]).addTo(window.map);
    }

    function errorLocation() {
        useDefaultLocation();
    }

    function useDefaultLocation() {
        console.warn("Using default location due to geolocation failure.");
        window.map.setCenter(defaultCoordinates);
        window.map.setZoom(15);
        new mapboxgl.Marker().setLngLat(defaultCoordinates).addTo(window.map);
    }
};

// Get directions between start and end points
window.getDirections = (token, endLng, endLat) => {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(position => {
                const startLng = position.coords.longitude;
                const startLat = position.coords.latitude;

                if (!window.map.loaded()) {
                    window.map.on('load', () => {
                        fetchDirections(token, startLng, startLat, endLng, endLat, resolve, reject);
                    });
                } else {
                    fetchDirections(token, startLng, startLat, endLng, endLat, resolve, reject);
                }
            }, () => {
                // If geolocation fails or permission is denied, use default coordinates
                fetchDirections(token, defaultCoordinates[0], defaultCoordinates[1], endLng, endLat, resolve, reject);
            }, {
                enableHighAccuracy: true
            });
        } else {
            // If geolocation is not supported, use default coordinates
            fetchDirections(token, defaultCoordinates[0], defaultCoordinates[1], endLng, endLat, resolve, reject);
        }
    });
};

// Fetch directions from Mapbox API and add route to map
function fetchDirections(token, startLng, startLat, endLng, endLat, resolve, reject) {
    const directionsRequest = `https://api.mapbox.com/directions/v5/mapbox/driving/${startLng},${startLat};${endLng},${endLat}?alternatives=true&geometries=geojson&language=en&overview=full&steps=true&access_token=${token}`;

    fetch(directionsRequest)
        .then(response => response.json())
        .then(data => {
            if (!data.routes || data.routes.length === 0) {
                console.error('No routes found.');
                reject('No routes found.');
                return;
            }

            const route = data.routes[0].geometry;
            const steps = data.routes[0].legs[0].steps;
            const duration = data.routes[0].duration;

            const routeGeoJSON = {
                'type': 'Feature',
                'geometry': route
            };

            // Check if source exists before removing it
            if (window.map.getSource('route')) {
                if (window.map.getLayer('route')) {
                    window.map.removeLayer('route');
                }
                window.map.removeSource('route');
            }

            window.map.addLayer({
                'id': 'route',
                'type': 'line',
                'source': {
                    'type': 'geojson',
                    'data': routeGeoJSON
                },
                'layout': {
                    'line-join': 'round',
                    'line-cap': 'round'
                },
                'paint': {
                    'line-color': '#ff0000',
                    'line-width': 5,
                    'line-opacity': 0.75
                }
            });

            // Clear old markers and add new step markers
            window.markers.forEach(marker => marker.remove());
            window.markers = [];

            steps.forEach((step, index) => {
                const dot = document.createElement('div');
                dot.className = 'route-dot';
                dot.innerText = index + 1;

                const marker = new mapboxgl.Marker({
                    element: dot,
                    anchor: 'bottom'
                })
                    .setLngLat(step.maneuver.location)
                    .setPopup(new mapboxgl.Popup({ offset: 25 })
                        .setText(`${index + 1}. ${step.maneuver.instruction}`))
                    .addTo(window.map);

                window.markers.push(marker);

                dot.addEventListener('click', () => {
                    window.map.flyTo({
                        center: step.maneuver.location,
                        zoom: 14
                    });
                });
            });

            // Ensure the instructions element exists
            const instructions = document.getElementById('instructions');
            if (instructions) {
                const tripInstructions = steps.map((step, index) => `<li>${step.maneuver.instruction}</li>`).join('');
                instructions.innerHTML = `<p><strong>Trip duration: ${Math.floor(duration / 60)} min</strong></p><ol>${tripInstructions}</ol>`;
            } else {
                console.error('Instructions element not found.');
            }

            resolve();
        })
        .catch(error => {
            console.error('Error fetching directions:', error);
            reject(error);
        });
}

// Function to fly to a specific location on the map
function flyToLocation(lng, lat, zoomLevel) {
    if (window.map) {
        window.map.flyTo({
            center: [lng, lat],
            zoom: zoomLevel,
            essential: true // This ensures the animation is smooth
        });
    } else {
        console.error("Map is not initialized.");
    }
}
