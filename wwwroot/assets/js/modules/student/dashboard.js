window.onload = async () => {
    renderLayout("student");

    await Promise.all([
        loadStats(),
        loadAssignments()
    ]);
};

// ================= STATS =================
async function loadStats() {
    try {
        const [classes, assignments, attempts] = await Promise.all([
            API.get("/student/my"),
            API.get("/assignment/student"),
            API.get("/submission/my") // dùng submission thay attempt
        ]);

        document.getElementById("myClasses").innerText = classes.length;
        document.getElementById("myAssignments").innerText = assignments.length;
        document.getElementById("myAttempts").innerText = attempts.length;

    } catch (err) {
        console.log("Dashboard load error", err);
    }
}

// ================= ASSIGNMENTS =================
async function loadAssignments() {

    const container = document.getElementById("assignmentList");

    // skeleton
    container.innerHTML = `
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
    `;

    try {
        const assignments = await API.get("/assignment/student");

        if (!assignments || assignments.length === 0) {
            container.innerHTML = `<p class="text-muted">No assignments</p>`;
            return;
        }

        // sort mới nhất
        assignments.sort((a, b) =>
            new Date(b.openTime) - new Date(a.openTime)
        );

        const latest = assignments.slice(0, 5);

        container.innerHTML = "";

        latest.forEach(a => {

            const card = document.createElement("div");
            card.className = "card";

            const now = new Date();
            const open = a.openTime ? new Date(a.openTime) : null;
            const close = a.closeTime ? new Date(a.closeTime) : null;

            let status = "";
            let action = "";

            if (open && now < open) {
                status = "Not open";
                action = `<button disabled>Not open</button>`;
            }
            else if (close && now > close) {
                status = "Closed";
                action = `<button onclick="review(${a.id})">Review</button>`;
            }
            else {
                status = "Available";
                action = `<button onclick="startExam(${a.id})">Start</button>`;
            }

            card.innerHTML = `
                <h3>${a.name}</h3>
                <p>${a.questionCount} questions</p>
                <p>Status: ${status}</p>
                ${action}
            `;

            container.appendChild(card);
        });

    } catch (err) {
        console.error(err);
        container.innerHTML = `<p class="text-danger">Load failed</p>`;
    }
}

// ================= ACTION =================
function startExam(id) {
    location.href = `/pages/student/exam.html?assignmentId=${id}`;
}

function review(id) {
    location.href = `/pages/student/review.html?assignmentId=${id}`;
}