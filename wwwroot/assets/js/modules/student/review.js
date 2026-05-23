const params =
    new URLSearchParams(location.search);

const assignmentId =
    params.get("assignmentId");

let reviewData = [];

// ===== INIT =====
window.onload = async () => {

    try {

        renderLayout("student");

        await loadReview();

    } catch (err) {

        console.error(err);

        document.getElementById(
            "reviewContainer"
        ).innerHTML = `
            <div class="card">
                Failed to load review
            </div>
        `;
    }
};

// ===== LOAD =====
async function loadReview() {

    try {

        const data =
            await API.request(
                "/exam/review/" + assignmentId
            );

        renderReview(data);

    } catch (err) {

        console.error(err);

        document.getElementById(
            "reviewContainer"
        ).innerHTML = `
            <div class="card">
                Review load failed
            </div>
        `;
    }
}

// ===== RENDER =====
function renderReview(list) {

    reviewData = list;

    const container =
        document.getElementById(
            "reviewContainer"
        );

    container.innerHTML = "";

    if (!list || list.length === 0) {

        container.innerHTML = `
            <div class="card">
                No review data
            </div>
        `;

        return;
    }

    list.forEach((q, index) => {

        const card =
            document.createElement("div");

        card.className = "card";

        let html = `
            <div class="question-title">
                Question ${index + 1}
            </div>

            <div class="question-content">
                ${q.content}
            </div>
        `;

        // ===== SINGLE CHOICE =====
        if (q.type === "single_choice") {

            html += `<div class="options">`;

            q.options.forEach(o => {

                const isCorrect =
                    o.label === q.correctAnswer;

                const isChosen =
                    o.label === q.studentAnswer;

                html += `
                    <div class="
                        option
                        ${isCorrect ? "correct" : ""}
                        ${isChosen ? "selected" : ""}
                    ">

                        <b>${o.label}.</b>
                        ${o.content}

                        ${isChosen
                            ? "(Your answer)"
                            : ""}

                    </div>
                `;
            });

            html += `</div>`;
        }

        // ===== FILL BLANK =====
        else {

            html += `
                <div class="option">

                    <b>Your answer:</b>
                    ${q.studentAnswer || "-"}

                </div>

                <div class="option correct">

                    <b>Correct answer:</b>
                    ${q.correctAnswer}

                </div>
            `;
        }

        // ===== STATUS =====
        html += `
            <div class="
                review-status
                ${q.isCorrect
                    ? "success"
                    : "fail"}
            ">

                ${q.isCorrect
                    ? "Correct"
                    : "Wrong"}

            </div>
        `;

        // ===== EXPLANATION =====
        if (q.explanation) {

            html += `
                <div class="explanation">

                    <b>Explanation:</b>

                    <br><br>

                    ${q.explanation}

                </div>
            `;
        }

        // ===== AI REVIEW =====
        html += `

            <div class="ai-review-box">

                <button
                    class="btn-secondary"
                    onclick="toggleAIBox(${index})">

                    Ask AI

                </button>

                <div
                    id="aiBox-${index}"
                    style="
                        display:none;
                        margin-top:15px;
                    ">

                    <input
                        id="aiQuestion-${index}"
                        class="form-input"
                        placeholder="Ask AI about this question">

                    <button
                        class="btn-primary"
                        style="margin-top:10px"
                        onclick="askAI(${index})">

                        Send

                    </button>

                    <div
                        id="aiResponse-${index}"
                        class="explanation"
                        style="margin-top:15px">
                    </div>

                </div>

            </div>
        `;

        card.innerHTML = html;

        container.appendChild(card);
    });
}

// ===== TOGGLE AI =====
function toggleAIBox(index) {

    const box =
        document.getElementById(
            "aiBox-" + index
        );

    if (box.style.display === "none") {

        box.style.display = "block";

    } else {

        box.style.display = "none";
    }
}

// ===== ASK AI =====
async function askAI(index) {

    try {

        const q =
            reviewData[index];

        const ask =
            document
            .getElementById(
                "aiQuestion-" + index
            )
            .value
            .trim();

        if (!ask)
            return alert("Enter question");

        const responseBox =
            document.getElementById(
                "aiResponse-" + index
            );

        responseBox.innerHTML =
            "AI is thinking...";

        const result =
            await API.request(
                "/ai/review-answer",
                "POST",
                {
                    question: q.content,

                    correctAnswer:
                        q.correctAnswer,

                    studentAnswer:
                        q.studentAnswer,

                    explanation:
                        q.explanation || "",

                    ask
                }
            );

        responseBox.innerHTML = `
            <b>AI Response:</b>
            <br><br>
            ${result.response}
        `;

    } catch (err) {

        console.error(err);

        alert("AI review failed");
    }
}

// ===== BACK =====
function goBack() {

    location.href =
        "/pages/student/assignment-menu.html?assignmentId="
        + assignmentId;
}
