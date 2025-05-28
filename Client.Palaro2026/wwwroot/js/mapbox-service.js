// Global variables
const sportsComplex = [125.940565259789, 8.55068128311271];
let userLat = null;
let userLong = null;
let userLocation = sportsComplex;
let clusterPopup = null;
let currentPopup = null;

// Blazor interaction functions
window.registerDotNetReference = function (dotNetRef) {
    window.dotNetRef = dotNetRef;
};

function sendVenueToBlazor(id) {
    if (window.dotNetRef) {
        window.dotNetRef.invokeMethodAsync('OnCoordinateSelected', id)
            .catch(err => console.error(err));
    } else {
        console.warn('Blazor reference not set.');
    }
}

// Map initialization
window.initializeMap = (containerId, token, markers) => {
    mapboxgl.accessToken = token;

    window.map = new mapboxgl.Map({
        container: containerId,
        style: 'mapbox://styles/mapbox/outdoors-v12',
        center: sportsComplex,
        zoom: 9,
        textColor: "#ffffff",
        antialias: true
    });

    const geojson = createGeoJSON(markers);
    setupMapControls();
    setupMapLayers(geojson);
    setupMapEvents();
};

// Helper functions for map initialization
function createGeoJSON(markers) {
    return {
        type: 'FeatureCollection',
        features: markers.map(location => ({
            type: 'Feature',
            geometry: {
                type: 'Point',
                coordinates: [location.longitude, location.latitude]
            },
            properties: {
                id: location.id,
                venue: location.venue || location.billetingQuarter,
                address: location.address,
                latitude: location.latitude,
                longitude: location.longitude
            }
        }))
    };
}

function setupMapControls() {
    const geolocateControl = new mapboxgl.GeolocateControl({
        positionOptions: { enableHighAccuracy: true },
        trackUserLocation: true,
        showUserHeading: true,
        showAccuracyCircle: true,
        showUserLocation: true
    });

    window.map.addControl(new mapboxgl.NavigationControl(), 'bottom-right');
    window.map.addControl(geolocateControl, 'bottom-right');

    geolocateControl.on('geolocate', (event) => {
        userLat = event.coords.latitude;
        userLong = event.coords.longitude;
        userLocation = [userLong, userLat];

        if (window.selectedDestination) {
            showDirections(userLocation, window.selectedDestination);
        }
    });
    // Trigger geolocation automatically once the map loads
    window.map.on('load', () => {
        geolocateControl.trigger(); // <-- this line triggers geolocation automatically
    });
}

function setupMapLayers(geojson) {
    window.map.on('load', () => {
        add3DBuildings();
        addMarkersSource(geojson);
        addClusterLayers();
        addUnclusteredLayers();
    });
}

function add3DBuildings() {
    const layers = window.map.getStyle().layers;
    const labelLayerId = layers.find(
        (layer) => layer.type === 'symbol' && layer.layout['text-field']
    ).id;

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
                'fill-extrusion-opacity': 1
            }
        },
        labelLayerId
    );
}

function addMarkersSource(geojson) {
    window.map.addSource('markers', {
        type: 'geojson',
        data: geojson,
        cluster: true,
        clusterMaxZoom: 14,
        clusterRadius: 50
    });
}

function addClusterLayers() {
    const clusterColors = ['#4CAF50', '#FF9800', '#F44336'];

    window.map.addLayer({
        id: 'clusters',
        type: 'circle',
        source: 'markers',
        filter: ['has', 'point_count'],
        paint: {
            'circle-color': [
                'step',
                ['get', 'point_count'],
                clusterColors[0],
                10, clusterColors[1],
                30, clusterColors[2]
            ],
            'circle-radius': [
                'step',
                ['get', 'point_count'],
                15,
                10, 20,
                30, 25
            ],
            'circle-stroke-width': 2,
            'circle-stroke-color': '#fff'
        }
    });

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
            'text-color': '#fff'
        }
    });
}

function addUnclusteredLayers() {
    window.map.addLayer({
        id: 'unclustered-point',
        type: 'circle',
        source: 'markers',
        filter: ['!', ['has', 'point_count']],
        paint: {
            'circle-color': '#007AFF',
            'circle-radius': 8,
            'circle-stroke-width': 2,
            'circle-stroke-color': '#fff'
        }
    });

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
            'text-color': '#333',
            'text-halo-color': '#fff',
            'text-halo-width': 2
        }
    });
}

function setupMapEvents() {
    setupMarkerClickEvent();
    setupClusterClickEvent();
}

function setupMarkerClickEvent() {
    window.map.on('click', 'unclustered-point', (e) => {
        const coordinates = e.features[0].geometry.coordinates.slice();
        const { id, venue, address } = e.features[0].properties;

        const popupContent = `
            <div class="popup-content" style="width: auto;">
                <h3>${venue}</h3>
                <h4>${address}</h4>
                <button onclick="sendVenueToBlazor(${id})">Get Direction</button>
            </div>
        `;

        new mapboxgl.Popup()
            .setLngLat(coordinates)
            .setHTML(popupContent)
            .addTo(window.map);
    });
}

function setupClusterClickEvent() {
    window.map.on('click', 'clusters', async (e) => {
        const features = window.map.queryRenderedFeatures(e.point, { layers: ['clusters'] });
        if (!features.length) return;

        const clusterId = features[0].properties.cluster_id;

        window.map.getSource('markers').getClusterExpansionZoom(clusterId, (err, zoom) => {
            if (!err) {
                window.map.easeTo({
                    center: features[0].geometry.coordinates,
                    zoom: zoom
                });
            }
        });

        window.map.getSource('markers').getClusterLeaves(clusterId, 50, 0, (err, leaves) => {
            if (err) return;

            const venues = leaves.map(leaf => ({
                name: leaf.properties.venue,
                address: leaf.properties.address,
                latitude: leaf.properties.latitude,
                longitude: leaf.properties.longitude,
                coordinates: leaf.geometry.coordinates
            })).filter(venue => venue.name);

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

                if (clusterPopup) clusterPopup.remove();

                clusterPopup = new mapboxgl.Popup()
                    .setLngLat(features[0].geometry.coordinates)
                    .setHTML(popupContent)
                    .addTo(window.map);
            }
        });
    });
}

// Map interaction functions
window.zoomToVenue = (lng, lat, venueName, address = "Address not available", closeClusterPopup = false) => {
    if (closeClusterPopup && clusterPopup) {
        clusterPopup.remove();
        clusterPopup = null;
    }

    if (currentPopup) currentPopup.remove();

    window.map.flyTo({
        center: [lng, lat],
        zoom: 15,
        essential: true
    });

    const popupContent = `
        <div class="popup-content" style="width: auto;">
            <h3>${venueName}</h3>
            <h4>${address}</h4>
            <button onclick="sendVenueToBlazor(${id})">Get Direction</button>
        </div>
    `;

    currentPopup = new mapboxgl.Popup()
        .setLngLat([lng, lat])
        .setHTML(popupContent)
        .addTo(window.map);
};

window.showDirections = (destination) => {
    if (!destination || destination.length !== 2) return;

    const origin = userLocation;
    const directionsUrl = `https://api.mapbox.com/directions/v5/mapbox/driving/${origin[0]},${origin[1]};${destination[0]},${destination[1]}?alternatives=true&geometries=geojson&language=en&overview=full&steps=true&access_token=${mapboxgl.accessToken}`;

    clearRoute();

    fetch(directionsUrl)
        .then(response => response.json())
        .then(data => {
            if (!data.routes || data.routes.length === 0) return;

            const route = data.routes[0].geometry.coordinates;

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


            fitRouteBounds(route);

            window.routeDestination = destination;
        })
        .catch(error => console.error('Error fetching directions:', error));
};

window.flyToLocation = function (lng, lat) {
    if (!window.map) return;

    window.map.flyTo({
        center: [lng, lat],
        essential: true,
        zoom: 18,
        pitch: 60,
        bearing: -60,
        speed: 1.2,
        curve: 1.2,
        duration: 5000
    });
};

window.centerMap = (userLocation, destination) => {
    const centerLocation = userLocation || [125.94130366163259, 8.55111787984696];
    window.map.setBearing(0);
    window.map.setPitch(0);
    window.map.setZoom(9);
    window.map.setCenter(centerLocation);
};

window.clearRoute = () => {
    if (window.map.getLayer('route')) window.map.removeLayer('route');
    if (window.map.getSource('route')) window.map.removeSource('route');
};

window.fitRouteBounds = (coordinates) => {
    if (!coordinates || coordinates.length < 2 || !window.map) return;

    const bounds = coordinates.reduce(
        (b, coord) => b.extend(coord),
        new mapboxgl.LngLatBounds(coordinates[0], coordinates[0])
    );

    window.map.fitBounds(bounds, {
        padding: 200,
        linear: true
    });
};

// Set selected location from Blazor
window.setSelectedLocation = function (lng, lat) {
    window.selectedLocation = [lng, lat];
};

// Fit map bounds using userLocation and selectedLocation
window.fitUserAndSelectedBounds = () => {
    if (!window.map || !window.userLocation || !window.selectedLocation) return;

    const coordinates = [window.userLocation, window.selectedLocation];

    const bounds = coordinates.reduce(
        (b, coord) => b.extend(coord),
        new mapboxgl.LngLatBounds(coordinates[0], coordinates[0])
    );

    window.map.fitBounds(bounds, {
        padding: 200,
        linear: true
    });
};

window.setUserLocationFromGeolocation = function () {
    if (!navigator.geolocation) {
        console.warn("Geolocation is not supported by this browser.");
        return;
    }

    navigator.geolocation.getCurrentPosition(
        (position) => {
            const lng = position.coords.longitude;
            const lat = position.coords.latitude;
            window.userLocation = [lng, lat];
            console.log("User location set from Geolocation:", window.userLocation);
        },
        (error) => {
            console.error("Error getting user location:", error);
        }
    );
};


