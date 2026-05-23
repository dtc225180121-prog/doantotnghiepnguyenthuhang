const Sidebar = {

renderTeacher() {

return `

<div class="sidebar">

<div class="logo-text" style="font-size: 1.5rem; font-weight: 900; color: #6b21a8; margin-right: 50px;">English Hub</div>

<button class="sidebar-item" 
onclick="Router.goTeacherClasses()">
Classes
</button>

<button class="sidebar-item" 
onclick="Router.goAssignments()">
Assignments
</button>

<button class="sidebar-item" 
onclick="Router.goProfile()">
Profile
</button>

<button class="sidebar-item" 
onclick="logout()" 
style="background: #fff1f2 !important; color: #e11d48 !important; margin-left: auto;">
Logout
</button>

</div>

`;

},

renderStudent() {

return `

<div class="sidebar">

<div class="logo-text" style="font-size: 1.5rem; font-weight: 900; color: #6b21a8; margin-right: 50px;">English Hub</div>

<button class="sidebar-item" 
onclick="Router.goStudentClasses()">
Classes
</button>

<button class="sidebar-item" 
onclick="Router.goProfile()">
Profile
</button>

<button class="sidebar-item" 
onclick="logout()"
style="background: #fff1f2 !important; color: #e11d48 !important; margin-left: auto;">
Logout
</button>

</div>

`;

}

};

window.Sidebar = Sidebar;

if (!window.logout) {
    window.logout = function logout() {
        Storage.clear();
        window.location.href = "/pages/auth/login.html";
    };
}
