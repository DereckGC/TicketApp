using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Repository;

namespace TicketApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;
        // Se define el repositorio que tiene que recibir el controlador 
        // para acceder sus metodos y realizar conexion con la base de datos

        public HomeController(ILogger<HomeController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;

        }

        //Funcion dl controlador para retornar a la vista de inex
        public async Task<IActionResult> Index()
        {

            string userRole = HttpContext.Session.GetString("Role");
            //Y se obtiene el rol del usuario que se dio al ingresar por la pantalla de inicio



            UserModel user = await GetCurrentUser();
            // Se obtiene el usuario actual
           
            if (user == null)
            {


     

                return RedirectToAction("Login", "User");

            }
            //Devuelve el nombre del usuario en una variable 
            ViewData["Role"] = userRole;
            ViewData["Username"] = HttpContext.Session.GetString("Username");
            return View(user);
        }

        //Método para obtener el usuario actual que hizo log in
        public async Task<UserModel> GetCurrentUser()
        {
            // Funcion que obtiene el usuario actual comparando los usuarios de la base de datos
            // con el nombre que se obtuvo al iniciar sesion
          List<UserModel> users = await _userRepository.GetUsers();
            return users.FirstOrDefault(user => user.Username == HttpContext.Session.GetString("Username"));

        }


        public IActionResult AdminDashBoard()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Remueve todas las sesiones y devuelve a la pagina de inicio
            return RedirectToAction("Login","User");
        }


        public IActionResult Privacy()
        {
            return View();
        }

      
    }
}
