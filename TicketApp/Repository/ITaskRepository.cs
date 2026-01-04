using TicketApp.Models;

namespace TicketApp.Repository
{
    // Interfaz para la conexion entre el controlador y el repositorio de las tareas
    public interface ITaskRepository
    {
        Task <List<TaskModel>> GetAllTasks();
        public Task<bool> AddTask(TaskModel newTask);
        public Task<bool> DeleteTask(int taskID);
        public Task<bool> EditTask(TaskModel taskToEdit);
        public Task<TaskModel> GetTaskByID(int taskID);
        Task <List<TaskModel>> GetTasksByUser(string userName);
    }

}
