const priorityOrder = { "High": 1, "Medium": 2, "Low": 3 };  // Jerarquía de prioridad
const stateOrder = { "Finished": 1, "In Progress": 2, "Pending": 3 };  // Jerarquía de estado
// Se definen arreglos que definirarn el orden en el que ira el arreglo al seleccionar 
// la prioridad o el estado 

function changeOrder() {
    // Se obtiene el valor seleccionado en el dropdown (criterio de ordenamiento)
    const sortBy = document.getElementById("sortSelect").value;

    // Luego el contenedor donde están las tarjetas de tareas
    const cardRow = document.getElementById("cardRow");

    // Y se accede a la colección de elementos con la clase "task-card" por un array para poder ordenarlo
    const cards = Array.from(cardRow.getElementsByClassName("task-card"));

    // Posteriormente se ordenan las cards segun el criterio seleccionado
    // realizando una comparacion entre la tarea a con la tarea b
    const sortedCards = cards.sort((a, b) => {
        let comparison = 0; // Variable para almacenar el resultado de la comparación

        switch (sortBy) {
            case 'title':
                // Comparar títulos alfabéticamente utilizando localeCompare
                comparison = a.getAttribute('data-title').localeCompare(b.getAttribute('data-title'));
                break;

            case 'priority':
                // Comparar prioridades utilizando el objeto priorityOrder
                // Se usa la resta para obtener un valor positivo o negativo, indicando el orden
                // Si el valor es negativo entonces el A va antes que el B y viceversa
                comparison = priorityOrder[a.getAttribute('data-priority')] - priorityOrder[b.getAttribute('data-priority')];
                break;

            case 'state':
                // Comparar estados utilizando el objeto stateOrder
                comparison = stateOrder[a.getAttribute('data-state')] - stateOrder[b.getAttribute('data-state')];
                break;

            case 'expirationDate':
                // Convertir las fechas de los atributos data-expirationDate a objetos Date
                const dateA = new Date(a.getAttribute('data-expirationDate'));
                const dateB = new Date(b.getAttribute('data-expirationDate'));

                // Comparar las fechas restando los objetos Date
                comparison = dateA - dateB;
                break;
        }

        return comparison; // Retornar el resultado de la comparación para que sort() ordene los elementos
    });

    // Vaciar el contenedor de tarjetas para actualizar el orden
    cardRow.innerHTML = '';

    // Volver a agregar las tarjetas en el nuevo orden
    sortedCards.forEach(card => cardRow.appendChild(card));
}
