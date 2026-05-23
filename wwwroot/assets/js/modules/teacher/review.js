const params = new URLSearchParams(location.search);

const assignmentId = params.get("assignmentId");
const studentId = params.get("studentId");

window.onload = async () =>
{
    renderLayout("teacher");

    const data =
        await API.request(
            `/result/review/${assignmentId}/${studentId}`
        );

    render(data);
};

function render(list)
{
    const container =
        document.getElementById("reviewList");

    container.innerHTML = "";

    list.forEach((q, index) =>
    {
        const div = document.createElement("div");
        div.className = "question-card";

        let optionsHtml = "";

        if(q.options && q.options.length)
        {
            const letters = ["A","B","C","D"];

            q.options.forEach((o,i)=>
            {
                const letter = letters[i];

                const isCorrect =
                    letter === q.correctAnswer;

                const isSelected =
                    letter === q.answer;

                optionsHtml += `
                    <div class="
                        option
                        ${isCorrect ? "correct" : ""}
                        ${isSelected ? "selected" : ""}
                    ">
                        ${letter}. ${o}
                    </div>
                `;
            });
        }
        else
        {
            optionsHtml = `
                <div class="option ${q.isCorrect ? "correct" : "wrong"}">
                    Student: ${q.answer}
                </div>
                <div class="option correct">
                    Correct: ${q.correctAnswer}
                </div>
            `;
        }

        div.innerHTML = `
            <div class="question-title">
                Q${index+1}. ${q.content}
            </div>

            <div>${optionsHtml}</div>

            <div class="question-footer">
                <span>${q.explanation || ""}</span>
            </div>
        `;

        container.appendChild(div);
    });
}
