function allowDrop(event) {
    event.preventDefault();
}

function drag(event) {
    event.dataTransfer.setData("text", event.target.id);
}
// Funciones para el drag and drop para evitar que actualice la pagina y enviar dentro del data el elemento que se transfiere

function drop(event) {
    event.preventDefault();

    if (getTask() == null) {
        return;
        // Funcion que verifica si actualmente la tarea no es nula,
        // ya que si lo fuera, es por que el usuario la elimino entonces no hay nada que arrastrar
    }

    // Funcion que despliega la pantalla de carga para realizar el envio de datos al controlador
    displayChargeScreen();

    // Espera 2 segundos antes de cerrar la pantalla de carga
    setTimeout(() => {
        closeChargeScreen();
    }, 2000); 

    //Obtiene el elemento del html al cual se movera el card de la tarea 
    let target = event.target;

    // Asegurarse de que el target es un contenedor válido
    while (!target.classList.contains("drag-drop-section") && target.parentNode) {
        target = target.parentNode;
    }

    let targetId = target.id; // ID del drop zone
    let taskId = event.dataTransfer.getData("text"); // ID del elemento arrastrado
    let taskElement = document.getElementById(taskId); // Obtener el elemento DOM

    if (target.classList.contains("drag-drop-section")) {
        target.appendChild(taskElement); // Ahora mueve el elemento al nuevo contenedor
    }

    // Y se verifica a cual estado va a cambiar la tarea a la hora de llamar al controlador
    switch (targetId) {
        case "progress":
            editTask("In Progress");
            break;

        case "pending":
            editTask("Pending");
            break;

        case "finished":
            editTask("Finished");
            break;

        default:
            break;
    }

}

function getTask() {
    // Funcion que verifica si el elemento de la task posee contenido o no
    // Si no lo posee es por que se elimino la tarea
    const taskContent = document.getElementById("taskContent");

    if (!taskContent || !taskContent.dataset.tasks) {
        return null;
    }
    return taskContent;
}

async function editTask(state) {
    // Una vez validado el cambio de estado
    const taskContent = getTask();
    const task = JSON.parse(taskContent.dataset.tasks);
    // Se obtiene la informacion guardada en el dataset del elemento de la tarea
    // y se parsea 
    task.State = state;
    // Luego se cambia el estado de la tarea por el que se se desea actualizar
    try {
        // Y se envia el fetch hacia el controlador para actualizar el objeto en la base de datps
        const response = await fetch("/Task/EditSateTask", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(task)
        });

        if (!response.ok) {
            const errorData = await response.json();
            return;
        } else {
            //Si el cambio de estado funciono, se actualiza la informacion en el card de la tarea
            let sateTask = document.getElementById("taskState"); // Obtiene el estado actual de la tarea
            sateTask.innerHTML = `<i class="bi bi-info-circle me-2"></i><strong>State:</strong> ${state}`;
        }

    } catch (error) {
        console.error("Request failed:", error);
    }
}

// Funcion para desplegar la pantalla de carga
function displayChargeScreen() {
    document.getElementById("spinner").classList.remove("d-none");
}

// Funcion para cerrar la pantalla de carga
function closeChargeScreen() {
    document.getElementById("spinner").classList.add("d-none");
}

// Funcion para desplegar la pantalla de edicion
function openEditModal(taskID) {
    // Mostrar el modal
    new bootstrap.Modal(document.getElementById("editTaskModal")).show();
}
// Funcion para ocultar la pantalla de edicion
function hideMessageScreen() {
    document.getElementById("screen").classList.add("d-none");
}

// Funcion para desplegar la pantalla de confirmar la eliminacion
function displayConfirmDelete() {
    console.log("Flag")
    document.getElementById("confirmDelete").classList.remove("d-none");
}

// Funcion para cerrar la pantalla de confirmar la eliminacion
function closeConfirmDelete() {
    document.getElementById("confirmDelete").classList.add("d-none");
}