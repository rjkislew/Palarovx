// Caution! Understand offline caveats before publishing: https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [
    /\.dll$/, /\.pdb$/, /\.wasm$/, /\.html$/, /\.js$/, /\.json$/, /\.css$/,
    /\.woff$/, /\.woff2$/, /\.png$/, /\.jpeg$/, /\.jpg$/, /\.gif$/,
    /\.ico$/, /\.blat$/, /\.webp$/, /\.dat$/
];

const offlineAssetsExclude = [/^service-worker\.js$/];

// Replace with your base path. Keep the trailing slash!
const base = "/palaro2026/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('[SW] Install');

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, {
            integrity: asset.hash,
            cache: 'no-cache'
        }));

    console.info('[SW] Caching files:', assetsRequests.map(r => r.url));

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

async function onActivate(event) {
    console.info('[SW] Activate');

    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );
}

async function onFetch(event) {
    if (event.request.method !== 'GET') return fetch(event.request);

    const cache = await caches.open(cacheName);

    // Handle navigation requests
    if (event.request.mode === 'navigate') {
        const cachedIndex = await cache.match('index.html');
        if (cachedIndex) {
            console.info('[SW] Serving cached index.html for navigation:', event.request.url);
            return cachedIndex;
        } else {
            console.warn('[SW] index.html not found in cache!');
        }
    }

    const cachedResponse = await cache.match(event.request);
    if (cachedResponse) {
        console.info('[SW] Serving from cache:', event.request.url);
        return cachedResponse;
    }

    return fetch(event.request);
}
