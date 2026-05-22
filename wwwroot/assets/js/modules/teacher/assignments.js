let deleteMode = false;
let selectedIds = new Set();

window.onload = async () => {
    renderLayout("teacher");
    showSkeleton();
    await loadAssignments();
};

// ===== SKELETON =====
function showSkeleton() {
    document.getElementById("assignmentList").innerHTML = `
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
    `;
}

// ===== LOAD =====
async function loadAssignments() {
    try {
        showSkeleton();

        const keyword =
            document.getElementById("keyword")?.value || "";

        const assignments =
            await API.request(
                "/assignment/my-assignments?keyword=" + keyword
            );

        render(assignments);
    }
    catch (err) {
        console.error(err);
    }
}

// ===== RENDER =====
function render(assignments) {
    const container = document.getElementById("assignmentList");
    container.innerHTML = "";

    if (!assignments || assignments.length === 0) {
        container.innerHTML = `<div class="card">No assignments</div>`;
        return;
    }

    assignments.forEach(a => {

    const count = a.questionCount ?? 0;
    const type = a.questionType ?? "unknown";

    const card = document.createElement("div");
    card.className = "assignment-card";

    card.innerHTML = `
        ${deleteMode ? `
            <input type="checkbox"
                   class="assignment-checkbox"
                   onchange="toggleSelect(${a.id}, this.checked)">
        ` : ""}

        <div class="assignment-title">
            ${a.name}
        </div>

        <div class="assignment-meta">
            Type: ${type}
        </div>

        <div class="assignment-meta">
            Questions: ${count}
        </div>

        <div class="assignment-actions">
            <button onclick="openQuestions(${a.id}, '${type}')">Q</button>
            <button onclick="assignClass(${a.id})">Assign</button>
            <button onclick="viewResult(${a.id})">Result</button>
        </div>
    `;

    container.appendChild(card);
});
}

// ===== DELETE MODE =====
function toggleDeleteMode() {
    deleteMode = !deleteMode;

    document.getElementById("confirmDeleteBtn").style.display =
        deleteMode ? "inline-block" : "none";

    selectedIds.clear();
    loadAssignments();
}

function toggleSelect(id, checked) {
    if (checked) selectedIds.add(id);
    else selectedIds.delete(id);
}

// ===== DELETE MULTI =====
async function deleteSelected() {
    if (selectedIds.size === 0)
        return alert("No selected");

    if (!confirm("Delete selected assignments?"))
        return;

    for (const id of selectedIds) {
        await API.request("/assignment/delete/" + id, "DELETE");
    }

    deleteMode = false;
    selectedIds.clear();

    loadAssignments();
}

// ===== MODAL =====
function openCreateModal() {
    document.getElementById("createModal").style.display = "flex";
}

function closeModal() {
    document.getElementById("createModal").style.display = "none";
}

// ===== CREATE =====
async function createAssignment() {
    try {
        const name =
            document.getElementById("assignmentName").value.trim();

        if (!Validator.assignmentName(name))
            return alert("Assignment name max 20 characters");

        const questionType =
            document.getElementById("questionType").value;

        const questionCount =
            document.getElementById("questionCount").value;

        if (!questionCount || questionCount <= 0)
            return alert("Question count must be > 0");

        const openTime =
            document.getElementById("openTime").value || null;

        const closeTime =
            document.getElementById("closeTime").value || null;

        const showResult =
            document.getElementById("showResult").checked;

        const showExplanation =
            document.getElementById("showExplanation").checked;

        await API.request("/assignment/create", "POST", {
            name,
            questionType,
            questionCount,
            openTime,
            closeTime,
            showResult,
            showExplanation
        });

        closeModal();
        loadAssignments();
    }
    catch (err) {
        console.error(err);
        alert("Create failed");
    }
}

// ===== NAVIGATION =====
function openQuestions(id, type) {
    location.href =
        "/pages/teacher/questions.html?assignmentId="
        + id
        + "&type="
        + type;
}

function viewResult(id) {
    location.href =
        "/pages/teacher/results.html?assignmentId=" + id;
}

function assignClass(id) {
    location.href =
        "/pages/teacher/assign-class.html?assignmentId=" + id;
}