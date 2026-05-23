const Router = {

redirectByRole() {

const role =
Storage.getRole();

if(
role ===
CONFIG.ROLES.TEACHER
)

location.href =
"/pages/teacher/dashboard.html";

else if(
role ===
CONFIG.ROLES.STUDENT
)

location.href =
"/pages/student/dashboard.html";

else

location.href =
"/pages/auth/login.html";

},


/* ================= AUTH ================= */

goLogin() {

location.href =
"/pages/auth/login.html";

},

goRegister() {

location.href =
"/pages/auth/register.html";

},


/* ================= TEACHER ================= */

goTeacherDashboard() {

location.href =
"/pages/teacher/dashboard.html";

},

goTeacherClasses() {

location.href =
"/pages/teacher/classes.html";

},

goAssignments() {

location.href =
"/pages/teacher/assignments.html";

},

goStudents() {

location.href =
"/pages/teacher/classes.html";

},

goAssignmentResults(assignmentId) {

location.href =

"/pages/teacher/results.html?assignmentId="

+ assignmentId;

},


/* ================= STUDENT ================= */

goStudentDashboard() {

location.href =
"/pages/student/dashboard.html";

},

goStudentClasses() {

location.href =
"/pages/student/my-classes.html";

},

goJoinClass() {

location.href =
"/pages/student/join-class.html";

},

goStudentAssignments(classId) {

location.href =

"/pages/student/assignments.html?classId="

+ classId;

},

goExam(assignmentId) {

location.href =

"/pages/student/exam.html?assignmentId="

+ assignmentId;

},

goHistory() {

location.href =
"/pages/student/history.html";

},

goResult(assignmentId) {

location.href =

"/pages/student/result.html?assignmentId="

+ assignmentId;

},


/* ================= SHARED ================= */

goProfile() {

location.href =
"/pages/shared/profile.html";

},

goChangePassword() {

location.href =
"/pages/shared/change-password.html";

}

};

window.Router = Router;
