-- Creación de la tabla Users
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100),
    IsAdmin BIT DEFAULT 0
);
DROP TABLE Users

-- Creación de la tabla Task
CREATE TABLE Task (
    ID INT IDENTITY PRIMARY KEY NOT NULL,
    Title VARCHAR(100) NOT NULL,
    TaskDescription VARCHAR(500) NOT NULL,
    ExpirationDate DATE CONSTRAINT CK_DATE CHECK (ExpirationDate >= CAST(GETDATE() AS DATE)),
    PriorityTask VARCHAR(50) NOT NULL CHECK (PriorityTask IN ('Low', 'Medium', 'High')),
    CreatedUser NVARCHAR(100) NOT NULL,
    AssignedUser NVARCHAR(100) NOT NULL,
    TaskState VARCHAR(100) NOT NULL CHECK (TaskState IN ('Pending', 'In Progress', 'Finished')),
    FOREIGN KEY (CreatedUser) REFERENCES Users(Username),
    FOREIGN KEY (AssignedUser) REFERENCES Users(Username)
);
DROP TABLE Task


-- Procedimiento para crear un usuario
CREATE PROCEDURE CreateUser
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Email NVARCHAR(100),
    @IsAdmin BIT
AS
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, IsAdmin)
    VALUES (@Username, @PasswordHash, @Email, @IsAdmin);
END;

-- Procedimiento para editar la información de un usuario
CREATE PROCEDURE EditUser
    @Id INT,
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Email NVARCHAR(100),
    @IsAdmin BIT
AS
BEGIN
    UPDATE Users
    SET 
        Username = @Username,
        PasswordHash = @PasswordHash,
        Email = @Email,
        IsAdmin = @IsAdmin
    WHERE Id = @Id;
END;

-- Procedimiento para obtener todos los usuarios
CREATE PROCEDURE GetAllUsers
AS
BEGIN
    SELECT Id, Username, Email, IsAdmin FROM Users;
END;
EXEC GetAllUsers;

-- Procedimiento para obtener los usuarios que no son administradores
CREATE PROCEDURE GetUsersNoAdmin
AS
BEGIN
    SELECT Id, Username, Email, IsAdmin FROM Users WHERE IsAdmin = 0;
END;
EXEC GetUsersNoAdmin;


-- Procedimiento para agregar una nueva tarea
CREATE PROCEDURE AddTask
    @Title NVARCHAR(100),
    @TaskDescription NVARCHAR(500),
    @ExpirationDate DATETIME,
    @PriorityTask NVARCHAR(50),
    @CreatedUser NVARCHAR(100),
    @AssignedUser NVARCHAR(100),
    @TaskState NVARCHAR(100)
AS
BEGIN
    INSERT INTO Task (Title, TaskDescription, ExpirationDate, PriorityTask, CreatedUser, AssignedUser, TaskState)
    VALUES (@Title, @TaskDescription, @ExpirationDate, @PriorityTask, @CreatedUser, @AssignedUser, @TaskState);
END;

-- Procedimiento para editar una tarea existente
CREATE PROCEDURE EditTask
    @TaskID INT,
    @Title VARCHAR(100),
    @TaskDescription VARCHAR(500),
    @ExpirationDate DATE,
    @PriorityTask VARCHAR(50),
    @AssignedUser NVARCHAR(100),
    @TaskState VARCHAR(100)
AS
BEGIN
    UPDATE Task
    SET 
        Title = @Title,
        TaskDescription = @TaskDescription,
        ExpirationDate = @ExpirationDate,
        PriorityTask = @PriorityTask,
        AssignedUser = @AssignedUser,
        TaskState = @TaskState
    WHERE ID = @TaskID;
END;

-- Procedimiento para eliminar una tarea
CREATE PROCEDURE DeleteTask
    @TaskID INT
AS
BEGIN
    DELETE FROM Task WHERE ID = @TaskID;
END;

-- Procedimiento para obtener todas las tareas
CREATE PROCEDURE GetAllTasks
AS
BEGIN
    SELECT * FROM Task;
END;
EXEC GetAllTasks;

-- Procedimiento para obtener una tarea por ID
CREATE PROCEDURE GetTaskByID
    @TaskID INT
AS
BEGIN
    SELECT * FROM Task WHERE ID = @TaskID;
END;
EXEC GetTaskByID @TaskID = 4;

-- Procedimiento para obtener tareas asignadas o creadas por un usuario
CREATE PROCEDURE GetTasksByUser
    @Username NVARCHAR(100)
AS
BEGIN
    SELECT * FROM Task
    WHERE AssignedUser = @Username OR CreatedUser = @Username;
END;
EXEC GetTasksByUser @Username = 'usuario1';


-- Procedimiento para iniciar sesión y obtener la contraseña y rol
CREATE PROCEDURE LoginUser
    @UsernameLogin NVARCHAR(100)
AS
BEGIN
    SELECT PasswordHash, IsAdmin FROM Users WHERE Username = @UsernameLogin;
END;

-- Eliminar el procedimiento LoginUser (si es necesario)
DROP PROCEDURE LoginUser;

