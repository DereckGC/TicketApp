

using System.Data;
using System.Net.Mail;
using System.Text;
using Microsoft.Data.SqlClient;
using TicketApp.Models;

namespace TicketApp.Repository
{
    public class EmailRepository : IEmailRepository
    {

        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public EmailRepository(string connectionString,IConfiguration configuration)
        {
            _connectionString = connectionString;
            _configuration = configuration;
        }



        public async Task<bool> InviteUser(string email)
        //Este metodo invita a un usuario a unirse a la aplicacion, se le envia un correo con una contraseña temporal
        {

            string randomPassword = GenerateRandomPassword();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);  // Hash the password


            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {

                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("CreateUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Username", email);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@IsAdmin", false);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0) // comprueba si se ha creado el usuario en la base de datos mediante el procedimiento almacenado y las cantidad de columnas afectadas
                        {
                            return await SendEmailInvitation(email, randomPassword);
                        }
                    }
                }

            }
            catch (SqlException ex)
            {
                return false;


            }
            return false;
        }//End invitation user method

        public String GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
            /*// Enumerable.Range(0, 6) crea una secuencia de 6 números: {0, 1, 2, 3, 4, 5}.
                // .Select(_ => chars[random.Next(chars.Length)]) 
                // Para cada número en {0, 1, 2, 3, 4, 5}, seleccionamos un carácter aleatorio de 'chars'.
                // random.Next(chars.Length) genera un índice aleatorio para seleccionar una letra o número de la cadena 'chars'.
                //
                // random.Next(chars.Length) genera un índice aleatorio para seleccionar una letra o número de la cadena 'chars'.
                //
                // Normalmente, .Select proporciona cada elemento como una variable (por ejemplo, 'i' en .Select(i => ...)).
                // Sin embargo, no necesitamos realmente el valor (solo nos interesa repetir una acción).
                // Usar '_' indica que estamos ignorando la variable, haciendo el código más limpio.
             */
        }// end Password generator method


        public async Task<bool> SendEmailInvitation(string email, string password)
        {
            try
            {
                //Optiene las credenciales desde el Json para luego poder setear los parametros para enviar el correo
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
               

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail); //En este caso se incluyo un correo personal para poder enviar los correos junto con las credenciales de la misma 
                    mail.To.Add(email);
                    mail.Subject = "Invitation to join TicketApp";
                    mail.Body = $"You have been invited to join TicketApp. Your temporary password is {password}. Please login and change your password.";
                   
                    using (SmtpClient smtp = new SmtpClient(smtpServer, port))
                    {
                        smtp.Port = port;
                        smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }


                }
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }

        }//End send email invitation 

        public async Task<bool>  SendEmailTask (TaskModel taskModel,string email,string reason)
        {
            /*Recibe un string ya que este metodo se utiliza para 2 razones distintas
             una para actualizar el estado de una Task y la otra para crear una nueva
            dependiendo del string que reciba, el contenid del mensaje del email*/
            string subject;
            string body;
            if (reason == "New Task")
            {
                subject = "You have been Added as  a collaborator for a new Task";
                 body = $"You have been added to a new task name:{taskModel.Title} by the user:{taskModel.CreatedUser} the new task is about {taskModel.Description} with a priority of:{taskModel.Priority} please check before the expiration date:{taskModel.ExpirationDate} ";
            } else
            {
                subject = "A task have been update recently";
                body = $"The Task {taskModel.Title} had change its state to {taskModel.State}, please check ASAP";
            }
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];


                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(email);
                    mail.Subject = subject;
                    mail.Body = body;

                    using (SmtpClient smtp = new SmtpClient(smtpServer, port))
                    {
                        smtp.Port = port;
                        smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);

                    }//SmtpClient
                }//using MailMessage

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
    
