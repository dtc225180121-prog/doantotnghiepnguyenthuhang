function logout() {

Storage.clear();

window.location.href =
"/pages/auth/login.html";

}

window.logout = logout;
