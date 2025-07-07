window.cookieService = {
    setCookie: function (key, value, expirationInSeconds, secure, sameSite) {
        let expiresText = "";

        // Handle expiration if provided
        if (expirationInSeconds > 0) {
            const date = new Date();
            date.setTime(date.getTime() + (expirationInSeconds * 1000));
            expiresText = "; expires=" + date.toDateString();
        }

        // Initialize cookie string with the key and value
        let cookieString = `${key}=${encodeURIComponent(value || "")}${expiresText}; path=/`;

        // Add Secure flag (if true)
        if (secure) {
            cookieString += "; Secure";
        }

        // Add SameSite flag (default is 'Lax', can be 'Strict' or 'None')
        cookieString += `; SameSite=${sameSite || 'Lax'}`;

        // Set the cookie
        document.cookie = cookieString;
    },

    getCookie: function (key) {
        const nameEQ = key + "=";
        return document.cookie.split(';').reduce((acc, c) => {
            c = c.trim();
            return c.indexOf(nameEQ) === 0 ? decodeURIComponent(c.substring(nameEQ.length)) : acc;
        }, null);
    },

    deleteCookie: function (key) {
        document.cookie = `${key}=; Max-Age=-99999999; path=/`;
    }
};
