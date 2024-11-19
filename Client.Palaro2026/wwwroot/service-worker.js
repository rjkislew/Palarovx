const CACHE_NAME = 'blazor-pwa-cache-v3';
const APP_SHELL = [
    '/', // Root path
    '/index.html', // Main HTML file
    '/manifest.webmanifest',
    '/Media/Icon/site.webmanifest',
    '/Media/Icon/favicon-32x32.png',
    '/Media/Icon/favicon-16x16.png',
    '/Media/Icon/apple-touch-icon.png',
    '/css/app.css',
    '/_content/MudBlazor/MudBlazor.min.css',
    '/js/cookieService.js',
    '/js/mapBoxService.js',
    '/js/getUserIPService.js',
    '/_framework/blazor.webassembly.js',
    '/_content/MudBlazor/MudBlazor.min.js',
];

const EXTERNAL_RESOURCES = [
    'https://api.mapbox.com/mapbox-gl-js/v3.6.0/mapbox-gl.css',
    'https://api.mapbox.com/mapbox-gl-js/v3.6.0/mapbox-gl.js',
];

// Install the service worker and cache essential files
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            const appShellCache = APP_SHELL.map((url) =>
                cache.add(url).catch((error) => {
                    console.error(`Failed to cache ${url}:`, error);
                })
            );

            const externalCache = EXTERNAL_RESOURCES.map((url) =>
                fetch(url, { mode: 'no-cors' })
                    .then((response) => cache.put(url, response))
                    .catch((error) => {
                        console.error(`Failed to fetch and cache ${url}:`, error);
                    })
            );

            return Promise.all([...appShellCache, ...externalCache]);
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
            return response || fetch(event.request).catch(() => {
                if (event.request.destination === 'document') {
                    return caches.match('/index.html');
                }
            });
        })
    );
});
