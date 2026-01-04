using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Repository;

namespace TicketApp.Controllers
{
    public class UserController : Controller
    {
        // GET: UserController

        private readonly IUserRepository _userRepository;
        private readonly IEmailRepository _emailRepository;


        public UserController(IUserRepository userRepository, IEmailRepository emailRepository)
        {
            _userRepository = userRepository;
            _emailRepository = emailRepository;
        }
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
       
            return View();
        }
        
        public async Task<IActionResult> GetUsers()
        {
            List<UserModel> users = await _userRepository.GetUsers();
            return PartialView("_GetUsers", users);
        }
        [HttpPost]
        //Metodo para actualizar el perfil del usuario
        public async Task<IActionResult> UpdateProfile([FromBody] Dictionary<string, string> requestBody)
        {
            //No hace falta que revise por el valor de la contraseña, ya que no es obligatorio cambiarla
            if (!requestBody.ContainsKey("Username") ||
                !requestBody.ContainsKey("Email") ||
                !requestBody.ContainsKey("ConfirmPassword"))
            {
                return BadRequest(new {message =  "Missing values fields"}); //Devuelve JSON con el mensaje de error hacia el javascript que maneja el editar
            }

            string username = requestBody["Username"];
            string email = requestBody["Email"];
            string password = requestBody["Password"];
            string confirmPassword = requestBody["ConfirmPassword"];

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized(new {message = "User not found"});
            }

            // Validar la contraseña actual
            string storedHashedPassword = await _userRepository.GetUserHashedPassword(currentUser.Username);
            if (!BCrypt.Net.BCrypt.Verify(confirmPassword, storedHashedPassword))
            {
                return BadRequest(new { message = "Current password is not correct" });
            }

            // Acualizar el perfil del usuario

            currentUser.Username = username;
            currentUser.Email = email;
           
            if (!string.IsNullOrEmpty(password))
            {
                currentUser.Password = BCrypt.Net.BCrypt.HashPassword(password); //En caso de que se quisiera cambiar la contraseña se incripta y se guarda en el nuevo usuario
            } else
            {
                currentUser.Password = storedHashedPassword; //En caso de que no se quiera cambiar la contraseña, se guarda la misma que ya estaba
            }

            bool updateSuccess = await _userRepository.EditUser(currentUser);
            if (updateSuccess)
            {
                HttpContext.Session.SetString("Username", currentUser.Username); //actualiza el nombre de usuario en la sesión
                return Ok(new { message = "Profile update" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update profile." });
            }
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserModel userModel)
        {
          
            if (!ModelState.IsValid)
            {
                //Valida el estado del modelo de usuario y si no es valido, devuelve un mensaje de error
                ModelState.AddModelError("","Invalid model of user!!");
                return View();
            }
            else
            {
                bool sucessfullRegister = await _userRepository.RegisterUser(userModel);//Conecta a la base de datos para registrar el usuario
                if (!sucessfullRegister)
                {
                    TempData["ErrorMessage"] = "Error trying to Register the user, Try Again";
                    return View();
                }
                else
                {
                    TempData["confirmMessage"] = "User registration sucessfull!!";
                    return View();
                }
            }
        }

        public IActionResult InviteUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InviteUser(string email)
        {
            bool success = await _emailRepository.InviteUser(email);
            if (success)
            {
                TempData["SuccessMessage"] = "Invitation send!";
                return View();
            }
            else
            {
                TempData["ErrorMessage"] = "Error sending invitation, Try again or check the email";
                return View();
            }

           
        }

        public async Task<IActionResult> InitializeAdmin()
        {
            // THIS ONLY BE CALL ONE TIME, YOU CALL IT FROM THE THE SEARCH BAR 
            await _userRepository.CreateDefaultAdmin();
            return Ok("Admin user created.");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            //validation of username and password

            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }




            var (isAuthenticated, isAdmin) = await _userRepository.LoginUser(username, password);





            if (isAuthenticated)
            {
             
              
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", isAdmin ? "Admin" : "User");


                
                if (isAdmin)
                    
                return RedirectToAction("AdminDashBoard", "Home");

                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                // Invalid login attempt
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }


        public async Task<UserModel> GetCurrentUser()
        {
            List<UserModel> users = await _userRepository.GetUsers();
            return users.FirstOrDefault(user => user.Username == HttpContext.Session.GetString("Username"));
        }






    }



}
