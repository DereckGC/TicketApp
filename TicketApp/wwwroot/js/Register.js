

//con este metodo se valida que las contraseñas sean iguales a la hora de regestrar un usuario, para asegurarse que la contraseña sea la correcta

function validatePassword(){
    const password = document.querySelector("#password").value;
    const confirmPass = document.querySelector("#confirmPassword").value;
    const alerPassword = document.querySelector("#alerPassword")
    if (password !== confirmPass) {
        alerPassword.textContent = "passwords are different, passwords should match"//Se muestra en la pantalla que las contraseñas no son iguales
        return false
    } else {
        alerPassword.textContent = "";
       
        return true;
    }

}