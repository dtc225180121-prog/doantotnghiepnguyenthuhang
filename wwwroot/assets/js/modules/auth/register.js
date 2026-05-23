async function register() {
    const api = window.API;
    const router = window.Router;
    const validator = window.Validator;

    const name = document.getElementById("name").value.trim();
    const email = document.getElementById("email").value.trim();
    const phone = document.getElementById("phone").value.trim();
    const password = document.getElementById("password").value.trim();
    const role = document.getElementById("role").value;

    if (!api || !router || !validator) {
        return alert("Core scripts are not loaded. Refresh the page and check the script paths.");
    }

    if (!validator.name(name)) {
        return alert("Invalid name: Must be 2-50 characters.");
    }

    if (!validator.email(email)) {
        return alert("Invalid email format.");
    }

    if (!validator.phone(phone)) {
        return alert("Invalid phone: Must be 10-11 digits.");
    }

    if (!validator.password(password)) {
        return alert("Invalid password: Must be 6-50 characters.");
    }

    try {
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

        alert("Register success");
        router.goLogin();
    }
    catch (err) {
        console.error(err);
        alert(err.message || "Register failed");
    }
}

window.register = register;
