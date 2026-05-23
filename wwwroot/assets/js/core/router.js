const Router = {

navigate(url) {

const destination =
new URL(url, window.location.origin);

if(destination.href === window.location.href)
return;

document.documentElement.classList.add("is-leaving");

window.setTimeout(() => {
window.location.href =
destination.href;
}, 140);

},

redirectByRole() {

const role =
Storage.getRole();

if(
role ===
CONFIG.ROLES.TEACHER
)

this.navigate("/pages/teacher/dashboard.html");

else if(
role ===
CONFIG.ROLES.STUDENT
)

this.navigate("/pages/student/dashboard.html");

else

this.navigate("/pages/auth/login.html");

},


/* ================= AUTH ================= */

goLogin() {

this.navigate("/pages/auth/login.html");

},

goRegister() {

this.navigate("/pages/auth/register.html");

},


/* ================= TEACHER ================= */

goTeacherDashboard() {

this.navigate("/pages/teacher/dashboard.html");

},

goTeacherClasses() {

this.navigate("/pages/teacher/classes.html");

},

goAssignments() {

this.navigate("/pages/teacher/assignments.html");

},

goStudents() {

this.navigate("/pages/teacher/classes.html");

},

goAssignmentResults(assignmentId) {

this.navigate(

"/pages/teacher/results.html?assignmentId="

+ assignmentId
);

},


/* ================= STUDENT ================= */

goStudentDashboard() {

this.navigate("/pages/student/dashboard.html");

},

goStudentClasses() {

this.navigate("/pages/student/my-classes.html");

},

goJoinClass() {

this.navigate("/pages/student/join-class.html");

},

goStudentAssignments(classId) {

this.navigate(

"/pages/student/assignments.html?classId="

+ classId
);

},

goExam(assignmentId) {

this.navigate(

"/pages/student/exam.html?assignmentId="

+ assignmentId
);

},

goHistory() {

this.navigate("/pages/student/history.html");

},

goResult(assignmentId) {

this.navigate(

"/pages/student/result.html?assignmentId="

+ assignmentId
);

},


/* ================= SHARED ================= */

goProfile() {

this.navigate("/pages/shared/profile.html");

},

goChangePassword() {

this.navigate("/pages/shared/change-password.html");

}

};

window.Router = Router;
