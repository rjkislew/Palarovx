importScripts('./service-worker-assets.js');

const CACHE_VERSION = self.assetsManifest.version;
const CACHE_NAME = `palaro2026-cache-${CACHE_VERSION}`;

const offlineAssetsInclude = [
    /\.dll$/, /\.pdb$/, /\.wasm$/, /\.html$/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/,
    /\.png$/, /\.jpe?g$/, /\.webp$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.svg$/,
    /tile\.mapbox\.com/, /\/attachments\//
];

const CRITICAL_FILES = [
    '/css', '/font', '/js', '/media', '/icon-192.png', '/icon-512.png', '/manifest.webmanifest', '/pwa-helper.js', '/service-worker.js',
    '/index.html'
];

// Send progress updates to Blazor app
function sendProgress(progress) {
    self.clients.matchAll().then(clients => {
        clients.forEach(client => {
            client.postMessage({ type: 'CACHE_PROGRESS', value: progress });
        });
    });
}

// Install — pre-cache essentials & start silent full caching
self.addEventListener('install', event => {
    console.log('[SW] Installing (Published)', CACHE_VERSION);

    event.waitUntil(
        caches.open(CACHE_NAME).then(cache =>
            cache.addAll(CRITICAL_FILES).then(() => {
                console.log('[SW] Critical files cached');
                silentCacheAllAssets();
            })
        )
    );

    self.skipWaiting();
});

// Activate — remove old caches
self.addEventListener('activate', event => {
    console.log('[SW] Activating', CACHE_VERSION);
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.map(key => {
                if (key !== CACHE_NAME) {
                    console.log('[SW] Deleting old cache', key);
                    return caches.delete(key);
                }
            }))
        )
    );
    self.clients.claim();
});

// Hybrid fetch strategy
self.addEventListener('fetch', event => {
    const request = event.request;
    if (!request.url.startsWith('http')) return; // Skip chrome-extension:// etc.

    // HTML → network-first
    if (request.mode === 'navigate') {
        event.respondWith(
            fetch(request).then(networkResponse => {
                if (networkResponse && networkResponse.ok) {
                    const clone = networkResponse.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                }
                return networkResponse;
            }).catch(() =>
                caches.match(request).then(resp => resp || caches.match('/offline.html'))
            )
        );
        return;
    }

    // Static assets & Mapbox → cache-first
    if (offlineAssetsInclude.some(pattern => pattern.test(request.url))) {
        event.respondWith(
            caches.match(request).then(cached => {
                if (cached) {
                    // Background update
                    fetch(request).then(networkResponse => {
                        if (networkResponse && networkResponse.ok) {
                            const clone = networkResponse.clone();
                            caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                        }
                    });
                    return cached;
                }
                // No cache yet → fetch and store
                return fetch(request).then(networkResponse => {
                    if (networkResponse && networkResponse.ok) {
                        const clone = networkResponse.clone();
                        caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                    }
                    return networkResponse;
                });
            })
        );
        return;
    }

    // Default → network-first
    event.respondWith(
        fetch(request).then(networkResponse => {
            if (networkResponse && networkResponse.ok) {
                const clone = networkResponse.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
            }
            return networkResponse;
        }).catch(() => caches.match(request))
    );
});

// Silent full caching after install
function silentCacheAllAssets() {
    const allAssets = self.assetsManifest.assets.map(asset =>
        new URL(asset.url, self.location).toString()
    );
    let cachedCount = 0;

    caches.open(CACHE_NAME).then(cache => {
        allAssets.reduce((chain, assetUrl) => {
            return chain.then(() => {
                return fetch(assetUrl).then(networkResponse => {
                    if (networkResponse && networkResponse.ok) {
                        const clone = networkResponse.clone();
                        cache.put(assetUrl, clone);
                    }
                    cachedCount++;
                    sendProgress(Math.round((cachedCount / allAssets.length) * 100));
                }).catch(err => {
                    console.warn('[SW] Failed to cache asset', assetUrl, err);
                    cachedCount++;
                    sendProgress(Math.round((cachedCount / allAssets.length) * 100));
                });
            });
        }, Promise.resolve()).then(() => {
            console.log('[SW] All assets cached');
            sendProgress(100);
        });
    });
}
