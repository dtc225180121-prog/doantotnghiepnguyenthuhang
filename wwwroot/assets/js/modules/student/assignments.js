console.log("ASSIGNMENTS READY");

const params = new URLSearchParams(location.search);
const classId = params.get("classId");

// ===== LOAD =====
async function loadAssignments() {
    const container = document.getElementById("assignmentList");

    try {
        if (!classId) {
            container.innerHTML = `<p class="text-danger">Missing classId</p>`;
            return;
        }

        showSkeleton();

        const assignments = await API.request(
            "/exam/assignments/" + classId
        );

        render(assignments);

    } catch (err) {
        console.error(err);
        container.innerHTML = `<p class="text-danger">Cannot load assignments</p>`;
    }
}

// ===== SKELETON =====
function showSkeleton() {
    const container = document.getElementById("assignmentList");
    container.innerHTML = "";

    for (let i = 0; i < 6; i++) {
        const div = document.createElement("div");

        div.innerHTML = `<div class="skeleton-card"></div>`;

        container.appendChild(div);
    }
}

// ===== CHECK STATUS =====
function getStatus(a) {
    const now = new Date();

    const open = a.openTime ? new Date(a.openTime) : null;
    const close = a.closeTime ? new Date(a.closeTime) : null;

    if (open && now < open) return "not_open";
    if (close && now > close) return "closed";
    if (a.submitted) return "done";

    return "available";
}

// ===== RENDER =====
function render(assignments) {
    const container = document.getElementById("assignmentList");
    container.innerHTML = "";

    if (!assignments || assignments.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <h4>No assignments yet</h4>
                <p class="text-muted">You're all caught up.</p>
            </div>
        `;
        return;
    }

    assignments.forEach(a => {
        const status = getStatus(a);

        let statusText = "";
        let btnText = "Start";
        let disabled = false;

        if (status === "not_open") {
            statusText = "Not opened yet";
            btnText = "Locked";
            disabled = true;
        }

        if (status === "closed") {
            statusText = "Closed";
            btnText = "Closed";
            disabled = true;
        }

        if (status === "done") {
            statusText = "Submitted";
            btnText = "View";
        }

        const div = document.createElement("div");
        div.className = "assignment-card";

        div.innerHTML = `
            <div>
                <div class="assignment-title">${a.name}</div>

                <p class="text-muted">${statusText}</p>

                <small>
                    ${a.openTime ? "Open: " + formatDate(a.openTime) : ""}
                    <br>
                    ${a.closeTime ? "Close: " + formatDate(a.closeTime) : ""}
                </small>
            </div>

            <button 
                ${disabled ? "disabled" : ""}
                onclick="openMenu(${a.id})">
                ${btnText}
            </button>
        `;

        container.appendChild(div);
    });
}

// ===== FORMAT DATE =====
function formatDate(d) {
    return new Date(d).toLocaleString();
}

// ===== NAV =====
function openMenu(assignmentId) {
    location.href =
        "/pages/student/assignment-menu.html?assignmentId=" +
        assignmentId;
}

// ===== INIT =====
document.addEventListener("DOMContentLoaded", loadAssignments);
