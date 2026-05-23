async function register() {
    const status = document.getElementById("registerStatus");
    const button = document.querySelector("#registerForm button");
    const api = window.API;
    const router = window.Router;
    const validator = window.Validator;

    const name = document.getElementById("name").value.trim();
    const email = document.getElementById("email").value.trim();
    const phone = document.getElementById("phone").value.trim();
    const password = document.getElementById("password").value.trim();
    const role = document.getElementById("role").value;

    if (!api || !router || !validator) {
        const message = "Core scripts are not loaded. Refresh the page and check the script paths.";
        if (status) status.textContent = message;
        return alert(message);
    }

    if (!validator.name(name)) {
        const message = "Invalid name: Must be 2-50 characters.";
        if (status) status.textContent = message;
        return alert(message);
    }

    if (!validator.email(email)) {
        const message = "Invalid email format.";
        if (status) status.textContent = message;
        return alert(message);
    }

    if (!validator.phone(phone)) {
        const message = "Invalid phone: Must be 10-11 digits.";
        if (status) status.textContent = message;
        return alert(message);
    }

    if (!validator.password(password)) {
        const message = "Invalid password: Must be 6-50 characters.";
        if (status) status.textContent = message;
        return alert(message);
    }

    try {
        if (status) status.textContent = "Creating account...";
        if (button) button.disabled = true;

        await api.request(
            "/auth/register",
            "POST",
            {
                name,
                email,
                phone,
                password,
                role
            }
        );

        if (status) status.textContent = "Account created. Redirecting...";
        router.goLogin();
    }
    catch (err) {
        console.error(err);
        const message = err.message || "Register failed";
        if (status) status.textContent = message;
        alert(message);
    }
    finally {
        if (button) button.disabled = false;
    }
}

window.register = register;

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("registerForm");

    if (form) {
        form.addEventListener("submit", (event) => {
            event.preventDefault();
            register();
        });
    }
});
