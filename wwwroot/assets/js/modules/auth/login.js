async function login() {

    const status = document.getElementById("loginStatus");
    const button = document.getElementById("loginBtn");

    try {

        if (status) status.textContent = "Logging in...";
        if (button) button.disabled = true;

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
            await API.request(
                "/auth/login",
                "POST",
                {
                    email,
                    password
                }
            );

        Storage.setToken(response.token);

        Storage.setRole(String(response.role || "").trim().toLowerCase());

        Router.redirectByRole();

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

    if (button) {

        button.addEventListener("click", () => login());

    }

});