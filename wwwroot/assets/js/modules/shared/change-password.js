window.onload = () => {
    renderLayout(Storage.getRole());
};

async function changePassword()
{
    const btn = document.getElementById("changeBtn");

    const oldPassword = document.getElementById("oldPassword").value.trim();
    const newPassword = document.getElementById("newPassword").value.trim();

    if (!oldPassword || !newPassword)
        return alert("Please fill all fields");

    if (newPassword.length < 6)
        return alert("New password must be at least 6 characters");

    btn.disabled = true;
    btn.innerText = "Changing...";

    try
    {
        await API.request("/user/change-password", "PUT", {
            oldPassword,
            newPassword
        });

        btn.innerText = "Success ✓";
        btn.classList.remove("btn-primary");
        btn.classList.add("btn-success");

        setTimeout(() => history.back(), 1200);
    }
    catch(err)
    {
        console.error(err);

        btn.disabled = false;
        btn.innerText = "Change Password";

        alert("Change password failed");
    }
}
