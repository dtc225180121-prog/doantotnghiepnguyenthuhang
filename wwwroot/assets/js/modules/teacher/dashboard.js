window.onload = async () => {
    renderLayout("teacher");

    await loadStats();
    await loadNotifications();
};

// ===== STATS =====
async function loadStats() {
    try {
        const classes = await API.get("/class/my");
        document.getElementById("totalClasses").innerText = classes.length;

        const assignments = await API.get("/assignment/my");
        document.getElementById("totalAssignments").innerText = assignments.length;

        const students = await API.get("/class/all-students");
        document.getElementById("totalStudents").innerText = students.length;

        const submissions = await API.get("/submission/my");
        document.getElementById("totalSubmissions").innerText = submissions.length;
    }
    catch (err) {
        console.log("Dashboard load error", err);
    }
}

// ===== NOTIFICATIONS =====
async function loadNotifications() {
    const container = document.getElementById("notifications");

    // skeleton
    container.innerHTML = `
        <div class="skeleton-line skeleton-title"></div>
        <div class="skeleton-line"></div>
        <div class="skeleton-line skeleton-small"></div>
    `;

    try {
        const submissions = await API.get("/submission/my");

        if (!submissions || submissions.length === 0) {
            container.innerHTML = `<p class="text-muted">No recent activity</p>`;
            return;
        }

        const latest = submissions.slice(0, 5);

        container.innerHTML = "";

        latest.forEach(s => {
            const div = document.createElement("div");
            div.className = "mb-2 p-2 border rounded";

            div.innerHTML = `
                <strong>${s.studentName || "Student"}</strong> submitted<br>
                <small class="text-muted">${s.assignmentTitle || "Assignment"}</small>
            `;

            container.appendChild(div);
        });

    } catch (err) {
        console.error(err);
        container.innerHTML = `<p class="text-danger">Failed to load activity</p>`;
    }
}