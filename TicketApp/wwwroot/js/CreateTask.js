
//Funcion para desplegar la pantalla de exito a la hora de crear la tarea
function displaySuccesScreen() {
    document.querySelector('.form-register').style.display = 'none';
    document.getElementById('errorScreen').style.display = 'none';
    document.getElementById('successScreen').style.display = 'block';
}

//Funcion para desplegar la pantalla de error a la hora de crear la tarea
function displayErrorScreen() {
    document.querySelector('.form-register').style.display = 'none';
    document.getElementById('successScreen').style.display = 'none';
    document.getElementById('errorScreen').style.display = 'block';
}

//Funcion para reciniciar el formulario despues de crear la tarea
function resetForm() {
    document.getElementById('taskForm').reset();
    document.querySelector('.form-register').style.display = 'block';
    document.getElementById('successScreen').style.display = 'none';
    document.getElementById('errorScreen').style.display = 'none';
}