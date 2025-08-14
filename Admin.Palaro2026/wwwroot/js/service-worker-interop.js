window.swInterop = {
    hasServiceWorker: function () {
        return 'serviceWorker' in navigator;
    },

    listenForProgress: function (dotNetHelper) {
        if (!navigator.serviceWorker) return;

        navigator.serviceWorker.addEventListener('message', event => {
            if (event.data && event.data.type === 'CACHE_PROGRESS') {
                dotNetHelper.invokeMethodAsync('UpdateProgress', event.data.value);
            }
        });
    }
};
