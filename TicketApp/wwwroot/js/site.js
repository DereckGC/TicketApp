// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Este metodo se encarga de enviar la informacion del formulario de la actualizacion de la informacion del usuario para poder actualizarse
function saveUserSetting() {
    let user = {
        Username: document.getElementById("userName").value,
        Email: document.getElementById("userEmail").value,
        Password: document.getElementById("userPassword").value
    };
    const confirmPassword = document.getElementById("userConfirmPassword").value;
    //Se utiliza el fetch para poder enviar la informacion al controlador de User para que se pueda actualizar la informacion del usuario
    fetch('/User/UpdateProfile', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Username: user.Username,
            Email: user.Email,
            Password: user.Password,
            ConfirmPassword: confirmPassword
        })
    })
        .then(response => response.json().then(data => ({ status: response.status, body: data })))
        .then(({ status, body }) => {
            if (status === 200) {
                //se muestra un mensaje de que la informacion se actualizo correctamente en caso que se haya actualizado
                showAlert(body.message, "success");
                document.getElementById("userSettingsForm").reset();
               
            } else {
                showAlert(body.message, "danger");
            }
        })
        .catch(error => {
            console.error("Error:", error);
            showAlert("An unexpected error occurred. Please try again.", "danger");
        });
}
//Este metodo se encarga de mostrar un mensaje en pantalla en caso de que se haya actualizado la informacion del usuario o en caso de que haya ocurrido un error 
function showAlert(message, type) {
    const alertContainer = document.getElementById("alertContainer");
    alertContainer.innerHTML = message;
    alertContainer.className = `alert alert-${type} mt-3`;
    alertContainer.classList.remove("d-none");

    // Se oculta luego de 3 segundos
    setTimeout(() => {
        alertContainer.classList.add("d-none");
    }, 3000);
}
