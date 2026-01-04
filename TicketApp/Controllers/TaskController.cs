using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Repository;

namespace TicketApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private static List<UserModel> _users = new List<UserModel>();
        private static List<TaskModel> _tasks = new List<TaskModel>();
        private readonly IEmailRepository _emailRepository;
        //Se definen los atributos que  el controlador de la tarea utilizara
        public TaskController(ITaskRepository taskRepository, IUserRepository userRepository, IEmailRepository emailRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _emailRepository = emailRepository;
        }

        // Se declara la funcion que recolectara las tareas relacionadas al usuario en la base de datos
        private async Task GetTasksByUser() 
        {
            // Se pasa por parametros el nombre que se obtuvo del usuario al iniciar sesion
            _tasks = await _taskRepository.GetTasksByUser(HttpContext.Session.GetString("Username"));
        }

        // Metodo que obtiene el email del usuario actual
        public async Task<string?> GetCurrentEmail(string userName)
        {
            
            List<UserModel> users = await _userRepository.GetUsers();
            // Recolecta los usuarios en la base de datos y busca las coincidencias en el nombre
            UserModel? newUser = users.FirstOrDefault(user => user.Username == userName);

            return newUser?.Email; 
        }

        // Funcion que obtiene todos los usuarios de la base de datos, accediendo al repositorio de los usuarios
        private async Task GetAllUsers()  
        {
            // Se verifica si el usuario es administrador o no, ya que solo los administradores pueden 
            // agregar de colaborador a otro adminsitrador
            if (HttpContext.Session.GetString("Role").Equals("Admin"))
            {
                _users = await _userRepository.GetUsers();
            }
            else
            {
                _users = await _userRepository.GetUsersNoAdmin();
            }

            // Busca dentro de los usuarios encontrados el que tenga el mismo nombre del usuario en sesion
            var userToRemove = _users.FirstOrDefault(user => user.Username.Equals(HttpContext.Session.GetString("Username"), StringComparison.OrdinalIgnoreCase));
            // Si el usuario existe, lo elimina ya que es el mismo usuario que esta en sesion, entonces lo ideal es que el no aparezca para agregar de colaborador
            if (userToRemove != null)
            {
                _users.Remove(userToRemove);  // Elimina el usuario de la lista
            }

        }

        private async Task GetAllTasks()
        {
            _tasks = await _taskRepository.GetAllTasks();
        }

        // Página principal de tareas teniendo en cuenta el usuario
        // Retornara a la vista inicial de las tareas pero teniendo en cuenta las relacionadas
        // con el usuario en sesion
        public async Task<IActionResult> Index()
        {
            try
            {
                await this.GetTasksByUser();
                return View(_tasks);  // Aquí debe enviarse la lista de tareas, no un string.
            }
            catch (Exception ex)
            {
                return View(new List<TaskModel>()); // Regresa una lista vacía si hay un error
            }
        }


        public async Task<IActionResult> ManageTask()
        {
            try
            {
                // El controlador envia informacion al la vista de index
                // para que tenga que cuenta que entro como administrador entonces desplegara todas las tareas
                // para que el administrador pueda realizar las acciones
                ViewData["Message"] = "ManageTask";
                await this.GetAllTasks();
                return View("Index", _tasks);  
            }
            catch (Exception ex)
            {
                return View(new List<TaskModel>()); // Regresa una lista vacía si hay un error
            }
        }

        // Vista para crear una nueva tarea
        public async Task<IActionResult> Create()
        {
            ViewData["CurrentUser"] = HttpContext.Session.GetString("Username");
            await this.GetAllUsers();
            return View(_users);
            // Retorna a la vista con los usuarios necesarios para que pueda seleccionar el colaborador
            // y en el view dara envia el nombre del usuario actual para que tenga nocion del creador de la tarea
        }

        // Método para añadir una nueva tarea
        [HttpPost]
        public async Task<IActionResult> AddTask(TaskModel newTask)
        {
            // Dentro el view data envia el nombre del usuario en sesion
            ViewData["CurrentUser"] = HttpContext.Session.GetString("Username");

            if (newTask == null)
            {
                // Pero verifica si la tarea no es nula para poder aniadirla
                // sino envia un mensaje de error
                ViewData["Message"] = "Could not add task";
                return View("Create");
            }

            try
            {
                // Una vez verificada la integridad de la tarea
                // se procede a aniadirla al base de datos como una nueva tarea
                bool added = await _taskRepository.AddTask(newTask);
          
                ViewData["Message"] = added ? "Added Tasks" : "Could not add task";
                // Una vez realizadas la verificacion se guarda un mensaje para que se muestre el resultado en la vista

                if (added)
                {
                    string email = await GetCurrentEmail(newTask.UserCollaborator);
                    bool emailSend = await _emailRepository.SendEmailTask(newTask, email, "New Task");
                    // Una vez creada la tarea se envia la notificacion al usuario colaborador
                    email = await GetCurrentEmail(newTask.CreatedUser);
                    emailSend = await _emailRepository.SendEmailTask(newTask, email, "New Task");
                    //Del mismo modo se envia al creador
                }

            }
            catch (Exception ex)
            {
                // Si el agrear tare fallo se verifica si fue por la fecha, ya que ingreso una fecha anterior a la actual
                // y se envia un mensaje a la vista segun corresponda
                if (ex.Message.Contains("CK_DATE"))
                {
                    ViewData["Message"] = "Could not add task. The expiration date cannot be earlier than today.";
                }
                else
                {
                    ViewData["Message"] = "Could not add task";
                }
                
            }
            // Si todo salio bien, regresa a la vista de crear la tarea con los usuarios que tenia
            await this.GetAllUsers();
            return View("Create", _users);
        }

        //Abrir vista para editar tarea
        [HttpGet]
        public async Task<IActionResult> Edit(int taskID)
        {
            TaskModel task;

            try
            {
                task = await _taskRepository.GetTaskByID(taskID);
                // Se obtiene la tarea que se va a editar por su ID y se envia a la vista para realizar las acciones
                if (task != null)
                {
                    return View(task);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // Metodo para eliminar la tarea
        public async Task<IActionResult> DeleteTask(int taskID)
        {
            try
            {
                // Se envia al repositorio y se realiza el procedimiento para eliminarla
                // de ahi se envia un mensaje a la vista segun el resultado y pueda mostrar el mensaje de ecito o error
                bool deleted = await _taskRepository.DeleteTask(taskID);
                ViewData["Message"] = deleted ? "Success: Task deleted successfully" : "Error: Could not delete task";
                if (deleted)
                {
                    return View("Edit", new TaskModel());
                }
                else
                {
                    return RedirectToAction("Edit", new { taskID });
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "Error: Could not delete task";
                return NotFound();
            }
        }

        // Funcion para editar una tarea
        [HttpPost]
        public async Task<IActionResult> Editask(TaskModel task)
        {
            try
            {
                // Se envia la tarea que se desea editar al repositorio para que ejecute el procedimiento
                // y se envia un mensaje a la vista segun el resultado
                bool edited = await _taskRepository.EditTask(task);
                
                //metodo para enviar el email 
                ViewData["Message"] = edited ? "Success: Task edited successfully" : "Error: Could not edit task";
                if (edited)
                {
                    TaskModel taskEdited = await _taskRepository.GetTaskByID(task.TaskID);
                    return View("Edit", taskEdited);
                }
                else
                {
                    return View("Edit", task);
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "Error: Could not edit task";
                return View("Edit", task);
            }
        }



        // Editar estado tarea
        [HttpPost]
        public async Task<IActionResult> EditSateTask([FromBody] TaskModel task)
        {
            try
            {
                // Una vez recibida la tarea desde el fetch envia el modelo al repositorio
                // manteniendo el mismo ID y cambiando los datos que se hayan actualizado
                bool edited = await _taskRepository.EditTask(task);
                // Y se envia un mensaje a la vista segun el resultado en la base de datos

                if (edited)
                {
                    string email = await GetCurrentEmail(task.UserCollaborator);
                    bool emailSend = await _emailRepository.SendEmailTask(task, email, "Update");

                    email = await GetCurrentEmail(task.CreatedUser);
                    emailSend = await _emailRepository.SendEmailTask(task, email, "Update");

                    return Ok(new { message = "Task successfully edited" });
                }
                else
                {
                    return BadRequest(new { message = "Could not edit task" });
                }
             
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }



        public async Task<IActionResult> PartialManageTasks()
        {
            var tasks = await _taskRepository.GetAllTasks(); // Fetch tasks from DB
            return PartialView("_ManageTask", tasks);
        }

    }
}
