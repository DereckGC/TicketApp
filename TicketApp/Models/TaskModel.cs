
namespace TicketApp.Models
{
    public class TaskModel
    {
        public int TaskID { get; set; } = -1;
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Priority { get; set; }
        public string CreatedUser { get; set; }
        public string UserCollaborator { get; set; }
        public string State { get; set; }

        // Constructor vacío
        public TaskModel() { }

        // Constructor sin ID
        public TaskModel(string title, string description, DateTime expirationDate,
                         string priority, string createdUser, string userCollaborator, string state)
        {
            Title = title;
            Description = description;
            ExpirationDate = expirationDate;
            Priority = priority;
            CreatedUser = createdUser;
            UserCollaborator = userCollaborator;
            State = state;
        }

        // Constructor con ID
        public TaskModel(int taskID, string title, string description, DateTime expirationDate,
                         string priority, string createdUser, string userCollaborator, string state)
        {
            TaskID = taskID;
            Title = title;
            Description = description;
            ExpirationDate = expirationDate;
            Priority = priority;
            CreatedUser = createdUser;
            UserCollaborator = userCollaborator;
            State = state;
        }

        public override string ToString()
        {
            return $"TaskID: {TaskID}, Title: {Title}, Description: {Description}, " +
                   $"ExpirationDate: {ExpirationDate.ToString("yyyy-MM-dd")}, Priority: {Priority}, " +
                   $"CreatedUser: {CreatedUser}, UserCollaborator: {UserCollaborator}, State: {State}";
        }
    }
}

