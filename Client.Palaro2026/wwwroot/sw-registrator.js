// sw-registrator.js
if ('serviceWorker' in navigator) {
    const baseHref = document.querySelector('base')?.getAttribute('href') ?? '/';
    navigator.serviceWorker.register(baseHref + 'service-worker.published.js')
        .then(() => console.log('[SW] Registered'))
        .catch(err => console.error('[SW] Registration failed:', err));
}
