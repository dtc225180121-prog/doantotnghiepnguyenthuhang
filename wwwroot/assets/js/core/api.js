const API = {

async request(
endpoint,
method = "GET",
body = null
)
{

const token =

    const token = Storage.getToken();

    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), 10000);

    try {

        const options = {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            signal: controller.signal
        };

        if (token) {
            options.headers.Authorization = "Bearer " + token;
        }

        if (body) {
            options.body = JSON.stringify(body);
        }

        const response = await fetch(CONFIG.API_BASE + endpoint, options);

        if (!response.ok) {
            const text = await response.text();

            try {
                throw text ? JSON.parse(text) : { message: "Request failed" };
            }
            catch {
                throw {
                    message: text || "Request failed"
                };
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
            throw { message: "Server did not respond in time. Check Render logs and database connection." };
        }

        throw error;

    }
    finally {

        window.clearTimeout(timeoutId);

    }

},

// Helpers
async get(endpoint) {
    return this.request(endpoint, "GET");
},

async post(endpoint, body) {
    return this.request(endpoint, "POST", body);
},

async put(endpoint, body) {
    return this.request(endpoint, "PUT", body);
},

async delete(endpoint) {
    return this.request(endpoint, "DELETE");
}

};