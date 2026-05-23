function isActive(path) {
    return window.location.pathname.includes(path) ? "active" : "";
}

const Sidebar = {
    renderTeacher() {
        return `
            <nav class="sidebar" aria-label="Teacher navigation">
                <div class="sidebar-brand">
                    <div class="sidebar-brand-mark">EH</div>
                    <div>
                        <div class="sidebar-brand-title">English Hub</div>
                        <div class="sidebar-brand-subtitle">Teacher workspace</div>
                    </div>
                </div>

                <div class="sidebar-nav">
                    <button class="sidebar-item ${isActive("/teacher/dashboard.html")}" onclick="Router.goTeacherDashboard()">
                        <span class="sidebar-icon">D</span>
                        <span>Dashboard</span>
                    </button>
                    <button class="sidebar-item ${isActive("/teacher/classes.html") || isActive("/teacher/class-students.html") ? "active" : ""}" onclick="Router.goTeacherClasses()">
                        <span class="sidebar-icon">C</span>
                        <span>Classes</span>
                    </button>
                    <button class="sidebar-item ${isActive("/teacher/assignments.html") || isActive("/teacher/assignment-detail.html") || isActive("/teacher/questions.html") ? "active" : ""}" onclick="Router.goAssignments()">
                        <span class="sidebar-icon">A</span>
                        <span>Assignments</span>
                    </button>
                    <button class="sidebar-item ${isActive("/teacher/results.html") || isActive("/teacher/review.html") ? "active" : ""}" onclick="Router.goAssignments()">
                        <span class="sidebar-icon">R</span>
                        <span>Results</span>
                    </button>
                    <button class="sidebar-item ${isActive("/shared/profile.html") || isActive("/shared/change-password.html") ? "active" : ""}" onclick="Router.goProfile()">
                        <span class="sidebar-icon">P</span>
                        <span>Profile</span>
                    </button>
                </div>

                <div class="sidebar-spacer"></div>

                <button class="sidebar-item sidebar-logout" onclick="logout()">
                    <span class="sidebar-icon">L</span>
                    <span>Logout</span>
                </button>
            </nav>
        `;
    },

    renderStudent() {
        return `
            <nav class="sidebar" aria-label="Student navigation">
                <div class="sidebar-brand">
                    <div class="sidebar-brand-mark">EH</div>
                    <div>
                        <div class="sidebar-brand-title">English Hub</div>
                        <div class="sidebar-brand-subtitle">Student workspace</div>
                    </div>
                </div>

                <div class="sidebar-nav">
                    <button class="sidebar-item ${isActive("/student/dashboard.html")}" onclick="Router.goStudentDashboard()">
                        <span class="sidebar-icon">D</span>
                        <span>Dashboard</span>
                    </button>
                    <button class="sidebar-item ${isActive("/student/my-classes.html") || isActive("/student/assignments.html") || isActive("/student/assignment-menu.html") ? "active" : ""}" onclick="Router.goStudentClasses()">
                        <span class="sidebar-icon">C</span>
                        <span>Classes</span>
                    </button>
                    <button class="sidebar-item ${isActive("/student/result.html") || isActive("/student/review.html") ? "active" : ""}" onclick="Router.goStudentClasses()">
                        <span class="sidebar-icon">R</span>
                        <span>Results</span>
                    </button>
                    <button class="sidebar-item ${isActive("/shared/profile.html") || isActive("/shared/change-password.html") ? "active" : ""}" onclick="Router.goProfile()">
                        <span class="sidebar-icon">P</span>
                        <span>Profile</span>
                    </button>
                </div>

                <div class="sidebar-spacer"></div>

                <button class="sidebar-item sidebar-logout" onclick="logout()">
                    <span class="sidebar-icon">L</span>
                    <span>Logout</span>
                </button>
            </nav>
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
