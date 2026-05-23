const Guard = {

    requireLogin() {

        if (
            !Storage.getToken()
        )

            location.href =
                "/pages/auth/login.html";

    },

    requireTeacher() {

        if (

            Storage.getRole()
            !==
            CONFIG.ROLES.TEACHER

        )

            Router.redirectByRole();

    },

    requireStudent() {

        if (

            Storage.getRole()
            !==
            CONFIG.ROLES.STUDENT

        )

            Router.redirectByRole();

    }

};

window.Guard = Guard;
