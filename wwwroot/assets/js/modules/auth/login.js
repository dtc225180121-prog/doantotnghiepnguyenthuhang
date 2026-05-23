async function login() {

    const status = document.getElementById("loginStatus");
    const button = document.getElementById("loginBtn");
    const api = window.API;
    const storage = window.Storage;
    const router = window.Router;

    try {

        if (status) status.textContent = "Logging in...";
        if (button) button.disabled = true;

        if (!api || !storage || !router) {
            throw new Error("Core scripts are not loaded. Refresh the page and check the script paths.");
        }

        const email =
            document
            .getElementById("email")
            .value
            .trim();

        const password =
            document
            .getElementById("password")
            .value
            .trim();

        const response =
            await api.request(
                "/auth/login",
                "POST",
                {
                    email,
                    password
                }
            );

        storage.setToken(response.token);

        storage.setRole(String(response.role || "").trim().toLowerCase());

        router.redirectByRole();

    }

    catch (err) {

        console.error(err);

        const message = err?.message || "Login failed: Please check your credentials.";
        if (status) status.textContent = message;
        alert(message);

    }

    finally {

        if (button) button.disabled = false;

    }

}

window.login = login;

document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("loginForm");
    const button = document.getElementById("loginBtn");

    if (form) {

        form.addEventListener("submit", (event) => {

            event.preventDefault();
            login();

        });

    }

});
