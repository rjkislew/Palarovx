let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
});

window.pwaHelper = {
    isInstallPromptAvailable: function () {
        return deferredPrompt !== undefined;
    },
    isRunningStandalone: function () {
        const isStandalone = window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true;
        console.log("isRunningStandalone: " + isStandalone);  // Add this line for debugging
        return isStandalone;
    },
    installApp: async function () {
        if (deferredPrompt) {
            deferredPrompt.prompt();
            const { outcome } = await deferredPrompt.userChoice;
            deferredPrompt = null;
            return outcome === 'accepted';
        }
        return false;
    }
};
