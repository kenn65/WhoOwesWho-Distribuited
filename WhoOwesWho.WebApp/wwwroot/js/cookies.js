// wwwroot/js/cookies.js

window.cookieApi = {
    setCookies: async function (url, data) {
        await fetch(url, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });
    },

    getCookies: async function (url) {
        const response = await fetch(url, {
            method: "GET",
            credentials: "include"
        });

        return await response.json();
    },

    deleteCookies: async function (url) {
        await fetch(url, {
            method: "POST",
            credentials: "include"
        });
    }
};