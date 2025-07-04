//// Caution! Be sure you understand the caveats before publishing an application with
//// offline support. See https://aka.ms/blazor-offline-considerations

//self.importScripts('./service-worker-assets.js');
//self.addEventListener('install', event => event.waitUntil(onInstall(event)));
//self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
//self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
//self.addEventListener('message', event => onMessage(event)); // <-- Add this

//const cacheNamePrefix = 'offline-cache-';
//const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
//const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.webp$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
//const offlineAssetsExclude = [/^service-worker\.js$/];

//async function onInstall(event) {
//    console.info('Service worker: Install');

//    // Activate the new service worker immediately
//    self.skipWaiting();

//    const assetsRequests = self.assetsManifest.assets
//        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
//        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
//        .map(asset => new Request(asset.url, { integrity: asset.hash }));

//    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
//}

//async function onActivate(event) {
//    console.info('Service worker: Activate');

//    const cacheKeys = await caches.keys();
//    await Promise.all(cacheKeys
//        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
//        .map(key => caches.delete(key)));

//    // Notify clients that a new service worker has taken control
//    const clientsList = await self.clients.matchAll();
//    for (const client of clientsList) {
//        client.postMessage({ type: 'NEW_VERSION_AVAILABLE' });
//    }
//}

//async function onFetch(event) {
//    let cachedResponse = null;
//    if (event.request.method === 'GET') {
//        const shouldServeIndexHtml = event.request.mode === 'navigate';
//        const request = shouldServeIndexHtml ? 'index.html' : event.request;
//        const cache = await caches.open(cacheName);
//        cachedResponse = await cache.match(request);
//    }

//    return cachedResponse || fetch(event.request);
//}

//function onMessage(event) {
//    if (event.data && event.data.type === 'SKIP_WAITING') {
//        console.info('Service worker: skipping waiting and activating new version.');
//        self.skipWaiting();
//    }
//}

self.addEventListener('install', event => {
    //console.info('Service worker: Install');
    self.skipWaiting(); // Immediately activate the new service worker
});

self.addEventListener('activate', event => {
    //console.info('Service worker: Activate');
    event.waitUntil(clearAllCaches()); // Optional: clear existing caches
    self.clients.claim();
});

self.addEventListener('fetch', event => {
    // Always fetch from network
    event.respondWith(fetch(event.request));
});

self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        //console.info('Service worker: Skipping waiting.');
        self.skipWaiting();
    }
});

// Utility to delete all existing caches (optional)
async function clearAllCaches() {
    const keys = await caches.keys();
    await Promise.all(keys.map(key => caches.delete(key)));
}
