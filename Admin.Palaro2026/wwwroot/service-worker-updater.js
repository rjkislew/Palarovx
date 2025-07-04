if ('serviceWorker' in navigator) {
    navigator.serviceWorker.addEventListener('message', event => {
        if (event.data?.type === 'NEW_VERSION_AVAILABLE') {
            // Silently activate new service worker and reload
            navigator.serviceWorker.controller.postMessage({ type: 'SKIP_WAITING' });
        }
    });

    // When the new service worker takes control, reload automatically
    navigator.serviceWorker.addEventListener('controllerchange', () => {
        window.location.reload();
    });
}
