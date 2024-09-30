const CACHE_NAME = 'blazor-pwa-cache-v1';
const MAPBOX_URLS = [
    'https://api.mapbox.com/mapbox-gl-js/v2.3.1/mapbox-gl.js',
    'https://api.mapbox.com/mapbox-gl-js/v2.3.1/mapbox-gl.css',
    'https://api.mapbox.com/styles/v1/mapbox/outdoors-v12',
    // Add other Mapbox resources like tiles, images, etc.
];

// Install the service worker and cache Mapbox files
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(MAPBOX_URLS);
        })
    );
});

// Activate the service worker and clean up old caches if necessary
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.filter((cacheName) => cacheName !== CACHE_NAME)
                    .map((cacheName) => caches.delete(cacheName))
            );
        })
    );
});

// Intercept network requests and serve from cache if available
self.addEventListener('fetch', (event) => {
    event.respondWith(
        caches.match(event.request).then((response) => {
            return response || fetch(event.request);
        })
    );
});
