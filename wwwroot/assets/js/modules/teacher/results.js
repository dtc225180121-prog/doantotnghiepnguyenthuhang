const params = new URLSearchParams(location.search);

const assignmentId = params.get("assignmentId");

let rawResults = [];

if(!assignmentId)
{
    alert("Missing assignmentId");
    location.href = "/pages/teacher/assignments.html";
}

window.onload = async () =>
{
    renderLayout("teacher");
    await loadResults();
};

// ================= LOAD =================
async function loadResults()
{
    try
    {
        const results =
            await API.request(
                "/result/assignment/" + assignmentId
            );

        render(results);
    }
    catch(err)
    {
        console.error(err);
        alert("Load results failed");
    }
}

// ================= RENDER =================
function render(results)
{
    rawResults = results;

    const container =
        document.getElementById("resultList");

    container.innerHTML = "";

    if(!results.length)
    {
        container.innerHTML =
            `<tr><td colspan="4">No submissions</td></tr>`;
        return;
    }

    results.forEach(r =>
    {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${r.name}</td>
            <td>${r.email}</td>
            <td><b>${r.score}</b></td>
            <td>${formatDate(r.submittedAt)}</td>
        `;

        // 🔥 CLICK → REVIEW
        tr.style.cursor = "pointer";

        tr.onclick = () =>
        {
            location.href =
            `/pages/teacher/review.html?assignmentId=${assignmentId}&studentId=${r.studentId}`;
        };

        container.appendChild(tr);
    });
}

// ================= FILTER =================
function filterResults()
{
    const keyword =
        document.getElementById("search")
        .value
        .toLowerCase();

    const filtered =
        rawResults.filter(r =>
            r.name.toLowerCase().includes(keyword) ||
            r.email.toLowerCase().includes(keyword)
        );

    render(filtered);
}

// ================= EXPORT =================
async function exportCSV()
{
    try {
        await API.download(
            "/result/export/" + assignmentId,
            "assignment-result.csv"
        );
    }
    catch (err) {
        console.error(err);
        alert(err.message || "Export failed");
    }
}

// ================= UTIL =================
function formatDate(date)
{
    return new Date(date)
        .toLocaleString();
}

function backExam()
{
    location.href =
        "/pages/teacher/assignments.html";
}
