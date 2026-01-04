using System.Data;
using Microsoft.Data.SqlClient;
using TicketApp.Models;

namespace TicketApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        // Constructor que recibe la cadena de conexión a la base de datos
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Método para registrar un nuevo usuario
        public async Task<bool> RegisterUser(UserModel userModel)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password);  // Cifra la contraseña
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Abre la conexión a la base de datos
                    using (SqlCommand command = new SqlCommand("CreateUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure; // Usa un procedimiento almacenado
                        // Agrega los parámetros necesarios para el procedimiento
                        command.Parameters.AddWithValue("@Username", userModel.Username);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        command.Parameters.AddWithValue("@Email", userModel.Email);
                        command.Parameters.AddWithValue("@IsAdmin", userModel.IsAdmin);

                        int rowsAffected = await command.ExecuteNonQueryAsync(); // Ejecuta el comando
                        return rowsAffected > 0; // Devuelve verdadero si se insertó el usuario
                    }
                }
            }
            catch (Exception ex)
            {
                return false; // Si ocurre un error, retorna falso
            }
        }

        // Método para iniciar sesión
        public async Task<(bool isAuthenticated, bool isAdmin)> LoginUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Abre la conexión

                    using (SqlCommand command = new SqlCommand("LoginUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure; // Usa procedimiento almacenado
                        command.Parameters.AddWithValue("@UsernameLogin", username); // Agrega el parámetro de usuario

                        using (SqlDataReader reader = await command.ExecuteReaderAsync()) // Lee los resultados
                        {
                            if (!await reader.ReadAsync()) // Si no encuentra el usuario, retorna falso
                            {
                                return (false, false);
                            }

                            string storedPasswordHash = reader["PasswordHash"].ToString(); // Obtiene la contraseña cifrada
                            bool isAdmin = reader["IsAdmin"] != DBNull.Value && Convert.ToBoolean(reader["IsAdmin"]); // Verifica si es administrador
                            bool isAuthenticated = BCrypt.Net.BCrypt.Verify(password, storedPasswordHash); // Verifica la contraseña

                            return (isAuthenticated, isAuthenticated && isAdmin); // Devuelve si está autenticado y si es administrador
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ocurrió un error al iniciar sesión: {ex.Message}");
            }

            return (false, false); // Retorna falso en caso de error
        }

        // Método para crear un usuario administrador por defecto si no existe
        public async Task CreateDefaultAdmin()
        {
            string adminUsername = "admin"; // Usuario administrador
            string adminPassword = "admin"; // Contraseña predeterminada
            string adminEmail = "admin@example.com"; // Correo del administrador

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(); // Abre la conexión

                // Verifica si ya existe un administrador en la base de datos
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", adminUsername); // Agrega el parámetro de nombre de usuario
                    var result = await command.ExecuteScalarAsync(); // Ejecuta la consulta
                    int count = Convert.ToInt32(result);

                    // Si no existe, crea un nuevo administrador
                    if (count == 0)
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword); // Cifra la contraseña

                        // Inserta el nuevo usuario administrador en la base de datos
                        string insertQuery = "INSERT INTO Users (Username, PasswordHash, Email, IsAdmin) VALUES (@Username, @PasswordHash, @Email, @IsAdmin)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Username", adminUsername);
                            insertCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                            insertCommand.Parameters.AddWithValue("@Email", adminEmail);
                            insertCommand.Parameters.AddWithValue("@IsAdmin", true); // Marca como administrador

                            await insertCommand.ExecuteNonQueryAsync(); // Ejecuta la inserción
                        }

                    }
                    else
                    {
                        Console.WriteLine("El usuario administrador ya existe.");
                    }
                }
            }
        }

        // Método para obtener la contraseña cifrada de un usuario
        public async Task<string> GetUserHashedPassword(string username)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(); // Abre la conexión
                using (SqlCommand command = new SqlCommand("GetUserHashedPassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; // Usa procedimiento almacenado
                    command.Parameters.AddWithValue("@Username", username); // Agrega el parámetro de nombre de usuario
                    return (string)await command.ExecuteScalarAsync(); // Devuelve la contraseña cifrada
                }
            }
        }

        // Método para obtener todos los usuarios
        public async Task<List<UserModel>> GetUsers()
        {
            List<UserModel> users = new List<UserModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Abre la conexión
                    using (SqlCommand command = new SqlCommand("GetAllUsers", connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync()) // Lee los usuarios
                        {
                            while (await reader.ReadAsync())
                            {
                                UserModel user = new UserModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                                };
                                users.Add(user); // Agrega el usuario a la lista
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
            }
            return users; // Retorna la lista de usuarios
        }

        // Método para editar un usuario
        public async Task<bool> EditUser(UserModel userModel)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Abre la conexión
                    using (SqlCommand command = new SqlCommand("EditUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure; // Usa procedimiento almacenado
                        // Agrega los parámetros necesarios para editar el usuario
                        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = userModel.Id });
                        command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Value = userModel.Username });
                        command.Parameters.Add(new SqlParameter("@PasswordHash", userModel.Password));
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object)userModel.Email ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter("@IsAdmin", SqlDbType.Bit) { Value = userModel.IsAdmin });

                        int rowsAffected = await command.ExecuteNonQueryAsync(); // Ejecuta la actualización
                        return rowsAffected > 0; // Retorna verdadero si se actualizó
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
                return false; // Retorna falso si ocurre un error
            }
        }

        // Método para obtener usuarios que no son administradores
        public async Task<List<UserModel>> GetUsersNoAdmin()
        {
            List<UserModel> users = new List<UserModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Abre la conexión
                    using (SqlCommand command = new SqlCommand("GetUsersNoAdmin", connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync()) // Lee los usuarios
                        {
                            while (await reader.ReadAsync())
                            {
                                UserModel user = new UserModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                                };
                                users.Add(user); // Agrega el usuario a la lista
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
            }
            return users; // Retorna la lista de usuarios no administradores
        }
    }
}
