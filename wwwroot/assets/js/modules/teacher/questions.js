const params = new URLSearchParams(location.search);

const assignmentId = params.get("assignmentId");
const assignmentType = params.get("type");

let editingId = null;
let currentQuestions = [];
let assignmentInfo = null;
let aiQuestions = [];

// ===== GUARD =====
if (!assignmentId || !assignmentType) {
    alert("Missing assignment info");
    location.href = "/pages/teacher/assignments.html";
}

// ===== INIT =====
window.onload = async () => {
    renderLayout("teacher");

    setupAssignmentTypeUI();
    showSkeleton();

    await loadAssignmentInfo();
    await loadQuestions();
    await loadClasses();
};

// ===== UI SETUP =====
function setupAssignmentTypeUI() {
    document.getElementById("assignmentTypeLabel").innerText =
        "Question type: " + assignmentType;

    if (assignmentType === "single_choice") {
        document.getElementById("optionsBox").style.display = "block";
        document.getElementById("fillAnswerBox").style.display = "none";

        document.getElementById("editOptionsBox").style.display = "block";
        document.getElementById("editFillBox").style.display = "none";
    } else {
        document.getElementById("optionsBox").style.display = "none";
        document.getElementById("fillAnswerBox").style.display = "block";

        document.getElementById("editOptionsBox").style.display = "none";
        document.getElementById("editFillBox").style.display = "block";
    }
}

// ===== SKELETON =====
function showSkeleton() {
    document.getElementById("questionList").innerHTML = `
        <tr><td colspan="4">Loading...</td></tr>
        <tr><td colspan="4">Loading...</td></tr>
    `;
}

// ===== TOGGLE CREATE =====
function toggleCreate() {
    const createBox = document.getElementById("createBox");
    const isHidden = createBox.style.display === "none";

    createBox.style.display = isHidden ? "block" : "none";
}

// ===== CREATE =====
async function createQuestion() {
    if (currentQuestions.length >= assignmentInfo.questionCount) {
    alert(
        "Maximum question count reached"
    );
    return;
    }

    try {
        const content = document.getElementById("content").value.trim();
        if (!content) return alert("Content required");

        let payload = {
            assignmentId,
            type: assignmentType,
            content,
            explanation: document.getElementById("explanation").value.trim()
        };

        if (assignmentType === "single_choice") {
            const A = document.getElementById("A").value.trim();
            const B = document.getElementById("B").value.trim();
            const C = document.getElementById("C").value.trim();
            const D = document.getElementById("D").value.trim();

            if (!A || !B || !C || !D)
                return alert("All options required");

            payload.correctAnswer =
                document.getElementById("correctAnswer").value.trim().toUpperCase();

            const q = await API.request("/question/create", "POST", payload);

            await API.request("/question/add-options", "POST", {
                questionId: q.id,
                A, B, C, D
            });
        } else {
            payload.correctAnswer =
                document.getElementById("fillCorrectAnswer").value.trim();

            await API.request("/question/create", "POST", payload);
        }

        resetForm();
        loadQuestions();

    } catch (err) {
        console.error(err);
        alert("Create failed");
    }
}

// ===== RESET =====
function resetForm() {
    document.getElementById("content").value = "";
    document.getElementById("explanation").value = "";

    ["A","B","C","D"].forEach(id => {
        if (document.getElementById(id))
            document.getElementById(id).value = "";
    });

    if (document.getElementById("correctAnswer"))
        document.getElementById("correctAnswer").value = "";

    if (document.getElementById("fillCorrectAnswer"))
        document.getElementById("fillCorrectAnswer").value = "";
}

// ===== LOAD =====
async function loadQuestions() {
    showSkeleton();

    try {
        const data =
            await API.request("/question/by-assignment/" + assignmentId);

        currentQuestions = data;
        renderQuestions(data);
        updateQuestionCounter();

    } catch (err) {
        console.error(err);
    }
}

// ===== RENDER (TABLE VERSION) =====
function renderQuestions(list) {
    const container = document.getElementById("questionList");
    container.innerHTML = "";

    if (!list || list.length === 0) {
        container.innerHTML = `
            <tr>
                <td colspan="4">No questions</td>
            </tr>
        `;
        return;
    }

    list.forEach((q, index) => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${index + 1}</td>
            <td>${q.content}</td>
            <td>${q.correctAnswer}</td>
            <td>
                <button onclick="openEdit(${q.id})"> Edit </button>

                <button onclick="generateExplanation(${q.id})"> AI Explain </button>

                <button onclick="deleteQuestion(${q.id})">Delete</button>
            </td>
        `;

        container.appendChild(tr);
    });
}

// ===== OPEN EDIT =====
function openEdit(id) {
    const q = currentQuestions.find(x => x.id === id);
    if (!q) return;

    editingId = id;

    document.getElementById("editContent").value = q.content;
    document.getElementById("editExplanation").value = q.explanation || "";

    if (q.type === "single_choice") {
        document.getElementById("editCorrect").value = q.correctAnswer;

        q.options?.forEach(o => {
            const el = document.getElementById("edit" + o.label);
            if (el) el.value = o.content;
        });
    } else {
        document.getElementById("editFill").value = q.correctAnswer;
    }

    document.getElementById("editModal").style.display = "flex";
}

// ===== CLOSE =====
function closeEdit() {
    document.getElementById("editModal").style.display = "none";
    editingId = null;
}

// ===== SAVE EDIT =====
async function saveEdit() {
    try {
        const content =
            document.getElementById("editContent").value.trim();

        if (!content) return alert("Content required");

        let correctAnswer;

        if (assignmentType === "single_choice") {
            correctAnswer =
                document.getElementById("editCorrect").value.trim().toUpperCase();
        } else {
            correctAnswer =
                document.getElementById("editFill").value.trim();
        }

        await API.request(
            "/question/update/" + editingId,
            "PUT",
            {
                content,
                correctAnswer,
                explanation:
                    document.getElementById("editExplanation").value
            }
        );

        // update options nếu là single_choice
        if (assignmentType === "single_choice") {
            const A = document.getElementById("editA").value.trim();
            const B = document.getElementById("editB").value.trim();
            const C = document.getElementById("editC").value.trim();
            const D = document.getElementById("editD").value.trim();

            await API.request(
                "/question/update-options/" + editingId,
                "PUT",
                { A, B, C, D }
            );
        }

        closeEdit();
        loadQuestions();

    } catch (err) {
        console.error(err);
        alert("Update failed");
    }
}

// ===== DELETE =====
async function deleteQuestion(id) {
    if (!confirm("Delete?")) return;

    await API.request("/question/delete/" + id, "DELETE");
    loadQuestions();
}

// ===== LOAD CLASSES =====
async function loadClasses() {

    try {

        const classes =
            await API.request("/class/my-classes");

        const container =
            document.getElementById("classList");

        container.innerHTML = "";

        classes.forEach(cls => {

            const div =
                document.createElement("div");

            div.className = "assign-item";

            div.innerHTML = `
                <span>${cls.name}</span>

                <button
                    id="assignBtn-${cls.id}"
                    onclick="assign(${cls.id})">
                    Assign
                </button>
            `;

            container.appendChild(div);
        });

    } catch (err) {

        console.error(err);

    }
}

// ===== ASSIGN =====
async function assign(classId) {

    const btn =
        document.getElementById(
            "assignBtn-" + classId
        );

    const confirmed =
        confirm("Assign this assignment to class?");

    if (!confirmed)
        return;

    try {

        btn.disabled = true;
        btn.innerText = "Assigning...";

        await API.request(
            "/assignment/assign-to-class",
            "POST",
            {
                assignmentId,
                classId
            }
        );

        btn.innerText = "Assigned";

        btn.classList.add("assigned");

    } catch (err) {

        console.error(err);

        btn.disabled = false;
        btn.innerText = "Assign";

        alert("Assign failed");
    }
}

function toggleAssign() {
    document
        .getElementById("assignModal")
        .classList
        .add("show");
}

function closeAssignModal() {
    document
        .getElementById("assignModal")
        .classList
        .remove("show");
}

async function loadAssignmentInfo() {
    try {

        assignmentInfo =
            await API.request(
                "/assignment/" + assignmentId
            );

        updateQuestionCounter();

    } catch (err) {
        console.error(err);
    }
}

function updateQuestionCounter() {

    if (!assignmentInfo)
        return;

    const current =
        currentQuestions.length;

    const max =
        assignmentInfo.questionCount;

    const counter =
        document.getElementById("questionCounter");

    const createBtn =
        document.getElementById("createBtn");

    counter.innerText =
        `${current} / ${max} questions`;

    // ===== FULL =====
    if (current >= max) {

        counter.classList.add("danger-text");

        createBtn.disabled = true;

        createBtn.innerText =
            "Question limit reached";
    }

    // ===== AVAILABLE =====
    else {

        counter.classList.remove("danger-text");

        createBtn.disabled = false;

        createBtn.innerText =
            editingId
            ? "Update Question"
            : "Create Question";
    }
}

async function generateAIQuestions() {

    try {

        const topic =
            document
            .getElementById("aiTopic")
            .value
            .trim();

        if (!topic)
            return alert("Topic required");

        const difficulty =
            document
            .getElementById("aiDifficulty")
            .value;

        const count =
            parseInt(
                document
                .getElementById("aiCount")
                .value
            );

        if (!count || count <= 0)
            return alert("Invalid count");

        const btn =
            document.querySelector(
                '[onclick="generateAIQuestions()"]'
            );

        btn.disabled = true;

        btn.innerText = "Generating...";

        const result =
            await API.request(
                "/ai/generate-questions",
                "POST",
                {
                    topic,
                    difficulty,
                    type: assignmentType,
                    count
                }
            );

        aiQuestions = result;

        renderAIPreview();

    } catch (err) {

        console.error(err);

        alert(
            err?.message ||
            "AI generation failed"
        );

    } finally {

        const btn =
            document.querySelector(
                '[onclick="generateAIQuestions()"]'
            );

        btn.disabled = false;

        btn.innerText =
            "Generate AI Questions";
    }
}

function renderAIPreview() {

    const box =
        document.getElementById(
            "aiPreviewBox"
        );

    const list =
        document.getElementById(
            "aiPreviewList"
        );

    list.innerHTML = "";

    if (!aiQuestions.length) {

        box.style.display = "none";

        return;
    }

    box.style.display = "block";

    aiQuestions.forEach((q, index) => {

        const div =
            document.createElement("div");

        div.className = "card";

        div.style.marginTop = "15px";

        let optionsHtml = "";

        if (q.options) {

            q.options.forEach(o => {

                optionsHtml += `
                    <div>
                        <b>${o.label}</b>.
                        ${o.content}
                    </div>
                `;
            });
        }

        div.innerHTML = `
            <div style="margin-bottom:10px;">
                <b>Question ${index + 1}</b>
            </div>

            <div>
                ${q.content}
            </div>

            <div style="margin-top:10px;">
                ${optionsHtml}
            </div>

            <div style="margin-top:10px;">
                <b>Correct:</b>
                ${q.correctAnswer}
            </div>

            <div style="margin-top:10px;">
                <b>Explanation:</b>
                ${q.explanation || ""}
            </div>
        `;

        list.appendChild(div);
    });
}

async function saveAIQuestions() {

    try {

        if (!aiQuestions.length)
            return;

        const btn =
            document.querySelector(
                '[onclick="saveAIQuestions()"]'
            );

        btn.disabled = true;

        btn.innerText = "Saving...";

        for (const q of aiQuestions) {

            const created =
                await API.request(
                    "/question/create",
                    "POST",
                    {
                        assignmentId,
                        type: assignmentType,
                        content: q.content,
                        correctAnswer:
                            q.correctAnswer,
                        explanation:
                            q.explanation
                    }
                );

            if (
                assignmentType ===
                "single_choice"
            ) {

                const A =
                    q.options.find(
                        x => x.label === "A"
                    )?.content || "";

                const B =
                    q.options.find(
                        x => x.label === "B"
                    )?.content || "";

                const C =
                    q.options.find(
                        x => x.label === "C"
                    )?.content || "";

                const D =
                    q.options.find(
                        x => x.label === "D"
                    )?.content || "";

                await API.request(
                    "/question/add-options",
                    "POST",
                    {
                        questionId: created.id,
                        A,
                        B,
                        C,
                        D
                    }
                );
            }
        }

        alert("AI questions saved");

        clearAIPreview();

        await loadQuestions();

    } catch (err) {

        console.error(err);

        alert("Save failed");

    } finally {

        const btn =
            document.querySelector(
                '[onclick="saveAIQuestions()"]'
            );

        btn.disabled = false;

        btn.innerText = "Save All";
    }
}

function clearAIPreview() {

    aiQuestions = [];

    document
        .getElementById("aiPreviewList")
        .innerHTML = "";

    document
        .getElementById("aiPreviewBox")
        .style.display = "none";
}

async function generateExplanation(id) {

    try {

        const q =
            currentQuestions.find(
                x => x.id === id
            );

        if (!q)
            return;

        let wrongAnswers = "";

        if (q.options?.length) {

            wrongAnswers =
                q.options
                .filter(x =>
                    x.label !== q.correctAnswer
                )
                .map(x => x.content)
                .join(", ");
        }

        const result =
            await API.request(
                "/ai/generate-explanation",
                "POST",
                {
                    question: q.content,
                    correctAnswer:
                        q.correctAnswer,
                    wrongAnswers
                }
            );

        await API.request(
            "/question/update/" + id,
            "PUT",
            {
                content: q.content,
                correctAnswer:
                    q.correctAnswer,
                explanation:
                    result.explanation
            }
        );

        alert("Explanation generated");

        await loadQuestions();

    } catch (err) {

        console.error(err);

        alert(
            "Generate explanation failed"
        );
    }
}

console.log("QUESTIONS JS LOADED");
console.log(typeof generateAIQuestions);