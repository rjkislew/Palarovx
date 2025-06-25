// Caution! Understand offline caveats before publishing: https://aka.ms/blazor-offline-considerations

importScripts('service-worker-assets.js'); // ✅ Load assetsManifest

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [
    /\.dll$/, /\.pdb$/, /\.wasm$/, /\.html$/, /\.js$/, /\.json$/, /\.css$/,
    /\.woff$/, /\.woff2$/, /\.png$/, /\.jpeg$/, /\.jpg$/, /\.gif$/,
    /\.ico$/, /\.blat$/, /\.webp$/, /\.dat$/, /\.webmanifest$/, /\.manifest$/
];

const offlineAssetsExclude = [/^service-worker\.js$/];

const base = "/";
const baseUrl = new URL(base, self.origin);

async function onInstall(event) {
    console.info('[SW] Install started');

    const assetsToCache = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, {
            integrity: asset.hash,
            cache: 'no-cache'
        }));

    const cache = await caches.open(cacheName);
    console.info('[SW] Caching files:', assetsToCache.map(r => r.url));
    await cache.addAll(assetsToCache);

    self.skipWaiting();
}

async function onActivate(event) {
    console.info('[SW] Activate');

    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );

    self.clients.claim();
}

async function onFetch(event) {
    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    const cache = await caches.open(cacheName);

    if (event.request.mode === 'navigate') {
        const cachedIndex = await cache.match('index.html');
        if (cachedIndex) {
            console.info('[SW] Serving cached index.html for navigation:', event.request.url);
            return cachedIndex;
        }
        console.warn('[SW] index.html not found in cache!');
    }

    const cachedResponse = await cache.match(event.request);
    if (cachedResponse) {
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(event.request);

        // ✅ Avoid caching unsupported request URLs
        if (
            networkResponse &&
            networkResponse.ok &&
            event.request.url.startsWith('http')
        ) {
            cache.put(event.request, networkResponse.clone());
        }

        return networkResponse;
    } catch (error) {
        console.warn('[SW] Network fetch failed:', event.request.url, error);
        return new Response("Offline", { status: 503 });
    }
}

