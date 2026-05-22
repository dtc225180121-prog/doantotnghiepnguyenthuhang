async function login() {

    try {

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

        Storage.setRole(response.role);

        Router.redirectByRole();

    }

    catch (err) {

        console.error(err);

        alert(err.message || "Login failed: Please check your credentials.");

    }

}