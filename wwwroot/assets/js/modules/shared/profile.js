window.onload = () => {
    const role = Storage.getRole();
    renderLayout(role);
    loadProfile();
};

async function loadProfile() {
    try {
        const profile = await API.request("/user/profile");

        // form
        document.getElementById("name").value = profile.name || "";
        document.getElementById("email").value = profile.email || "";
        document.getElementById("phone").value = profile.phone || "";
        document.getElementById("role").value = profile.role || "";

        // display
        document.getElementById("displayName").innerText =
            profile.name || "User";

        document.getElementById("avatar").innerText =
            (profile.name || "U")[0].toUpperCase();

        const badge = document.getElementById("roleBadge");
        badge.innerText = profile.role;
        badge.classList.add(profile.role); // teacher / student

        // show form
        document.getElementById("loadingState").remove();
        document.getElementById("profileForm").classList.remove("hidden");

    } catch (err) {
        console.error(err);
        document.getElementById("loadingState").innerText =
            "Failed to load profile";
    }
}

async function updateProfile() {
    const btn = document.getElementById("updateBtn");

    const name = document.getElementById("name").value.trim();
    const phone = document.getElementById("phone").value.trim();

    if (!name) {
        alert("Name cannot be empty");
        return;
    }

    btn.disabled = true;
    btn.innerText = "Updating...";

    try {
        await API.request("/user/update-profile", "PUT", {
            name,
            phone
        });

        btn.innerText = "Updated";
        btn.classList.remove("btn-primary");
        btn.classList.add("btn-success");

        setTimeout(() => {
            btn.disabled = false;
            btn.innerText = "Update Profile";
            btn.classList.remove("btn-success");
            btn.classList.add("btn-primary");
        }, 1500);

    } catch (err) {
        console.error(err);
        btn.disabled = false;
        btn.innerText = "Update Profile";
        alert("Update failed");
    }
}

function goChangePassword() {
    location.href = "/pages/shared/change-password.html";
}
