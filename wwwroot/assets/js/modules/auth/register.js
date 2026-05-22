async function register()
{

const name =
document.getElementById("name").value.trim();

const email =
document.getElementById("email").value.trim();

const phone =
document.getElementById("phone").value.trim();

const password =
document.getElementById("password").value.trim();

const role =
document.getElementById("role").value;


if(!Validator.name(name))
return alert("Invalid name: Must be 2-50 characters.");

if(!Validator.email(email))
return alert("Invalid email format.");

if(!Validator.phone(phone))
return alert("Invalid phone: Must be 10-11 digits.");

if(!Validator.password(password))
return alert("Invalid password: Must be 6-50 characters.");


try
{

await API.request(

"/auth/register",

"POST",

{
name,
email,
phone,
password,
role
}

);

alert("Register success");

Router.goLogin();

}

catch(err)
{

console.error(err);

alert(

err.message ||

"Register failed"

);

}

}