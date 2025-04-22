window.localStorageHelper = {
    set: (key, value) => localStorage.setItem(key, value),
    get: (key) => {
        let data = localStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    },
    remove: (key) => localStorage.removeItem(key)
};
