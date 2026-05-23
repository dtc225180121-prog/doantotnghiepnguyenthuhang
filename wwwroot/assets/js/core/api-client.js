const API = {
    buildUrl(endpoint) {
        return CONFIG.API_BASE + endpoint;
    },

    buildHeaders() {
        const headers = {
            "Content-Type": "application/json"
        };

        const token = Storage.getToken();

        if (token) {
            headers.Authorization = "Bearer " + token;
        }

        return headers;
    },

    async request(endpoint, method = "GET", body = null) {
        const controller = new AbortController();
        const timeoutId = window.setTimeout(() => controller.abort(), 60000);

        try {
            const options = {
                method,
                headers: this.buildHeaders(),
                signal: controller.signal
            };

            if (body) {
                options.body = JSON.stringify(body);
            }

            const response = await fetch(this.buildUrl(endpoint), options);

            if (!response.ok) {
                if (response.status === 401) {
                    Storage.clear();
                    window.location.href = "/pages/auth/login.html";
                    throw { message: "Session expired. Please log in again." };
                }

                const text = await response.text();

                try {
                    throw text ? JSON.parse(text) : { message: "Request failed" };
                }
                catch {
                    throw { message: text || "Request failed" };
                }
            }

            const contentType = response.headers.get("content-type");

            if (contentType && contentType.includes("application/json")) {
                return await response.json();
            }

            return await response.text();
        }
        catch (error) {
            if (error && error.name === "AbortError") {
                throw { message: "Server is waking up. Please try again in a moment." };
            }

            throw error;
        }
        finally {
            window.clearTimeout(timeoutId);
        }
    },

    get(endpoint) {
        return this.request(endpoint, "GET");
    },

    post(endpoint, body) {
        return this.request(endpoint, "POST", body);
    },

    put(endpoint, body) {
        return this.request(endpoint, "PUT", body);
    },

    delete(endpoint) {
        return this.request(endpoint, "DELETE");
    },

    async download(endpoint, filename = "download") {
        const response = await fetch(this.buildUrl(endpoint), {
            method: "GET",
            headers: this.buildHeaders()
        });

        if (!response.ok) {
            const text = await response.text();
            throw { message: text || "Download failed" };
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement("a");

        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    }
};

window.API = API;
