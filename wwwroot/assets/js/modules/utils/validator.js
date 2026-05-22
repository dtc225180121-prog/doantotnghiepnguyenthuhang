const Validator = {

email(email) {

return /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
.test(email);

},

phone(phone) {

return /^[0-9]{10,11}$/
.test(phone);

},

name(name) {

return name && name.length >= 2 && name.length <= 50;

},

password(password) {

return /^.{6,50}$/
.test(password);

},

classCode(code) {

return /^[a-zA-Z0-9]{1,8}$/
.test(code);

},

assignmentName(name) {

return /^[a-zA-Z0-9 ]{1,20}$/
.test(name);

},

fillBlankAnswer(text) {

return /^[a-zA-Z]+$/
.test(text);

}

};