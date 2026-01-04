using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TicketApp.Models;

namespace TicketApp.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _sqlConnection;

        private readonly string procedureGetAllTasksts = "GetAllTasks";
        private readonly string procedureEditlTaskst = "EditTask";
        private readonly string procedureDeleteTaskst = "DeleteTask";
        private readonly string procedureAddTaskst = "AddTask";
        private readonly string procedureGetTaskByID = "GetTaskByID";
        private readonly string procedureGetTaskByUser = "GetTasksByUser";
        // Se declaran los procedimientos en las variables que usara el repositorio

        public TaskRepository(string connectionString, IConfiguration sqlConnection)
        {
            this._sqlConnection = sqlConnection;
            this._connectionString = connectionString;  
        } // Se inicializa la conexion con la base de datos

        public async Task<List<TaskModel>> GetAllTasks()
        {
            List<TaskModel> tasks = new List<TaskModel>();

            try
            {
                // Mediante la conexion se ejecuta el procedimiento para obtener todas las tareas
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {
                    await connection.OpenAsync(); 
                    using (SqlCommand cmd = new SqlCommand(this.procedureGetAllTasksts, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            // Y se van agregando a la lista que sera retornada
                            while (await reader.ReadAsync()) 
                            {
                                tasks.Add(new TaskModel(
                                    (int)reader["ID"],
                                    reader["Title"].ToString(),
                                    reader["TaskDescription"].ToString(),
                                    (DateTime)reader["ExpirationDate"],
                                    reader["PriorityTask"].ToString(),
                                    reader["CreatedUser"].ToString(),
                                    reader["AssignedUser"].ToString(), 
                                    reader["TaskState"].ToString()
                                ));
                            }
                        }
                    }
                    await connection.CloseAsync(); 
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return tasks;
        }


        public async Task<bool> AddTask(TaskModel newTask)
        {
            try
            {
                // Mediante la conexion con la base de datos se ejecuta el procedimiento que agregara una tarea
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {

                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(this.procedureAddTaskst, connection))
                    {
                        // Y se le pasan por parametros los datos de la tarea para que el procedimiento se ejecute
                        // correctamente
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Title", newTask.Title);
                        cmd.Parameters.AddWithValue("@TaskDescription", newTask.Description);
                        cmd.Parameters.AddWithValue("@ExpirationDate", newTask.ExpirationDate);
                        cmd.Parameters.AddWithValue("@PriorityTask", newTask.Priority);
                        cmd.Parameters.AddWithValue("@CreatedUser", newTask.CreatedUser);
                        cmd.Parameters.AddWithValue("@AssignedUser", newTask.UserCollaborator);
                        cmd.Parameters.AddWithValue("@TaskState", newTask.State);
                        await cmd.ExecuteNonQueryAsync();

                    }
                    await connection.CloseAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }

        public async Task<bool> DeleteTask(int taskID)
        {

            try
            {
                // Mediante la conexion con la base de datos se ejecuta el procedimiento que eliminara una tarea
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(this.procedureDeleteTaskst, connection))
                    {
                        // Pasandole por parametros el ID de la tarea que se quiere eliminar de la tabla 
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TaskID", taskID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await connection.CloseAsync() ;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }

        public async Task<bool> EditTask(TaskModel taskToEdit)
        {

            try
            {
                // Mediante la conexion con la base de datos se ejecuta el procedimiento que editara una tarea
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {
                    await connection.OpenAsync();
                    // Pasando por parametros los datos de la tarea junto con el ID para que el procedimiento pueda 
                    // actualizar a esa tarea en especifico
                    using (SqlCommand cmd = new SqlCommand(this.procedureEditlTaskst, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TaskID", taskToEdit.TaskID);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Title", taskToEdit.Title);
                        cmd.Parameters.AddWithValue("@TaskDescription", taskToEdit.Description);
                        cmd.Parameters.AddWithValue("@ExpirationDate", taskToEdit.ExpirationDate);
                        cmd.Parameters.AddWithValue("@PriorityTask", taskToEdit.Priority);
                        cmd.Parameters.AddWithValue("@AssignedUser", taskToEdit.UserCollaborator);
                        cmd.Parameters.AddWithValue("@TaskState", taskToEdit.State);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }

        public async Task<TaskModel> GetTaskByID(int TaskID)
        {
            TaskModel task = null;

            try
            {
                // Mediante la conexion con la base de datos se ejecuta el procedimiento que editara una tarea

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(this.procedureGetTaskByID, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TaskID", TaskID);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Verifica si hay datos antes de leer
                            {
                                task = new TaskModel(
                                    reader.GetInt32(reader.GetOrdinal("ID")), // Usa GetOrdinal para seguridad
                                    reader.GetString(reader.GetOrdinal("Title")),
                                    reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    reader.GetDateTime(reader.GetOrdinal("ExpirationDate")),
                                    reader.GetString(reader.GetOrdinal("PriorityTask")),
                                    reader.GetString(reader.GetOrdinal("CreatedUser")),
                                    reader.GetString(reader.GetOrdinal("AssignedUser")),
                                    reader.GetString(reader.GetOrdinal("TaskState"))
                                );
                            }
                            // Y una ves haya leido los datos de la tarea que se busca, la retorna
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la tarea con ID {TaskID}: {ex.Message}");
            }

            return task; // Retorna null si no encuentra la tarea
        }

        public async Task<List<TaskModel>> GetTasksByUser(string userName)
        {
            List<TaskModel> tasks = new List<TaskModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Ejecuta el procedimiento que obtendra las tareas que se relacionen con el nombre de un usuario
                    // ya sea que hayan sido creadas por el o siendo colaborador
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(this.procedureGetTaskByUser, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", userName);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            while (await reader.ReadAsync())
                            {
                                tasks.Add(new TaskModel(
                                    (int)reader["ID"],
                                    reader["Title"].ToString(),
                                    reader["TaskDescription"].ToString(),
                                    (DateTime)reader["ExpirationDate"],
                                    reader["PriorityTask"].ToString(),
                                    reader["CreatedUser"].ToString(),
                                    reader["AssignedUser"].ToString(),
                                    reader["TaskState"].ToString()
                                ));
                            }

                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return tasks;
        }

    }
}
