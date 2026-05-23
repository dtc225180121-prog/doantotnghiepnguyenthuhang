window.onload = async () =>
{
renderLayout("teacher");

showSkeleton();

await loadClasses();
};



function showSkeleton()
{
document.getElementById("classList").innerHTML =

`
<div class="skeleton-card">
<div class="skeleton-line skeleton-title"></div>
<div class="skeleton-line skeleton-small"></div>
</div>

<div class="skeleton-card">
<div class="skeleton-line skeleton-title"></div>
<div class="skeleton-line skeleton-small"></div>
</div>

<div class="skeleton-card">
<div class="skeleton-line skeleton-title"></div>
<div class="skeleton-line skeleton-small"></div>
</div>
`;
}



async function loadClasses()
{
try
{

showSkeleton();

const keyword =
document
.getElementById("keyword")
?.value || "";


const classes =
await API.request(
"/class/my-classes?keyword=" + encodeURIComponent(keyword)
);


renderClasses(classes);

}
catch(err)
{
console.error(err);
}
}

function renderClasses(classes)
{
    const container = document.getElementById("classList");
    container.innerHTML = "";

    if (!classes || classes.length === 0) {
        container.innerHTML = `<p>No classes found</p>`;
        return;
    }

    classes.forEach(cls =>
    {
        const div = document.createElement("div");
        div.className = "class-card";

        div.innerHTML = `
            <h3>${cls.name}</h3>
            <p>Code: ${cls.classCode}</p>
        `;

        const actions = document.createElement("div");
        actions.className = "class-actions";

        const btnStudents = document.createElement("button");
        btnStudents.innerText = "Students";
        btnStudents.onclick = () => goStudents(cls.id);

        const btnDelete = document.createElement("button");
        btnDelete.innerText = "Delete";
        btnDelete.onclick = () => deleteClass(cls.id);

        actions.appendChild(btnStudents);
        actions.appendChild(btnDelete);

        div.appendChild(actions);
        container.appendChild(div);
    });
}

async function createClass()
{

try
{

const name =
document
.getElementById("className")
.value
.trim();


if(!name)
return alert("Enter class name");


await API.request(
"/class/create",
"POST",
{ name }
);


loadClasses();

}
catch(err)
{
console.error(err);
alert("Create failed");
}

}



async function deleteClass(classId)
{

try
{

await API.request(
"/class/delete/" + classId,
"DELETE"
);

alert("Deleted successfully");

loadClasses();

}
catch(err)
{
console.error(err);
alert("Delete failed");
}

}



function goStudents(classId)
{

location.href =

"/pages/teacher/class-students.html?classId="

+ classId;

}
