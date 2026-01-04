using TicketApp.Models;

namespace TicketApp.Repository
{
    public interface IUserRepository
    {

  
     
        Task<(bool isAuthenticated, bool isAdmin)> LoginUser(string username, string password);
        Task CreateDefaultAdmin();
        Task<bool> RegisterUser(UserModel userModel);
        Task<bool> EditUser(UserModel userModel);
        Task <List<UserModel>> GetUsers();
        Task<List<UserModel>> GetUsersNoAdmin();
        Task<string> GetUserHashedPassword(string username);

    }
}
