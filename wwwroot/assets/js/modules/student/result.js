const params = new URLSearchParams(location.search);
const assignmentId = params.get("assignmentId");

// ===== LOAD =====
async function loadResult() {
    try {
        if (!assignmentId) {
            alert("Missing assignmentId");
            return;
        }

        // latest
        const result = await API.request(
            "/exam/result/" + assignmentId
        );

        renderScore(result);

        // history
        const history = await API.request(
            "/exam/history/" + assignmentId
        );

        renderHistory(history);

    } catch (err) {
        console.error(err);
        alert("Load failed");
    }
}

// ===== SCORE =====
function renderScore(result) {
    const box = document.getElementById("scoreBox");

    if (!result) {
        box.innerHTML = `<p>No result</p>`;
        return;
    }

    const score = result.score || 0;

    let status = "Fail";
    let cls = "fail";

    if (score >= 80) {
        status = "Excellent";
        cls = "excellent";
    }
    else if (score >= 50) {
        status = "Pass";
        cls = "pass";
    }

    box.innerHTML = `
        <div class="score-main ${cls}">
            <h1>${score}</h1>
            <p>${status}</p>
        </div>
    `;
}

// ===== HISTORY =====
function renderHistory(list) {
    const container = document.getElementById("history");
    container.innerHTML = "";

    if (!list || list.length === 0) {
        container.innerHTML =
            `<p class="text-muted">No attempts yet</p>`;
        return;
    }

    list.forEach((item, index) => {

        const div = document.createElement("div");
        div.className = "history-card";

        // highlight latest
        if (index === 0) {
            div.classList.add("latest");
        }

        div.innerHTML = `
            <div class="history-left">
                <b>Attempt ${index + 1}</b>
                <p>${formatTime(item.time)}</p>
            </div>

            <div class="history-right">
                <span class="score-badge">
                    ${item.score}
                </span>

                <button onclick="goReview(${index})">
                    Review
                </button>
            </div>
        `;

        container.appendChild(div);
    });
}

function goMenu() {

    location.href =
        "/pages/student/assignment-menu.html?assignmentId="
        + assignmentId;
}

// ===== FORMAT =====
function formatTime(t) {
    if (!t) return "N/A";

    const date = new Date(t);
    return date.toLocaleString();
}

// ===== NAV =====
function goReview(index) {
    location.href =
        "/pages/student/review.html?assignmentId=" +
        assignmentId +
        "&attempt=" + index;
}

function backExam() {
    location.href =
        "/pages/student/my-classes.html";
}

// ===== INIT =====
window.onload = async () => {
    renderLayout("student");
    await loadResult();
};
