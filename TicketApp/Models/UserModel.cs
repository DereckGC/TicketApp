namespace TicketApp.Models
{
    public class UserModel
    {
        public int Id { get; set; } // Primary Key
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; } = false; //default value


    }
}
