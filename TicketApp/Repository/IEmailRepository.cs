using TicketApp.Models;

namespace TicketApp.Repository
{
    public interface IEmailRepository
    {

        Task<bool> InviteUser(string email);
        String GenerateRandomPassword();
        Task<bool> SendEmailTask(TaskModel taskModel, string email, string reason);

    }
}
