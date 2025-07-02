using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using Supabase;
using Supabase.Gotrue;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proekt.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly Supabase.Client _supabase;

        public AuthController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // 1. Аутентификация в Supabase
                var session = await _supabase.Auth.SignIn(model.Email, model.Password);

                if (session?.User == null)
                {
                    ModelState.AddModelError("", "Неверный email или пароль");
                    return View(model);
                }

                // 2. Получаем данные пользователя из таблицы users
                var userResponse = await _supabase
                    .From<UserProfile>()
                    .Where(u => u.Email == model.Email)
                    .Single();

                if (userResponse == null)
                {
                    ModelState.AddModelError("", "Профиль пользователя не найден");
                    return View(model);
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userResponse.Id.ToString()),
            new Claim(ClaimTypes.Email, userResponse.Email),
            new Claim(ClaimTypes.Role, userResponse.Role),
            new Claim("full_name", userResponse.FullName ?? string.Empty)
        };

                var identity = new ClaimsIdentity(claims, "Supabase");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                // 4. Перенаправление по роли
                return userResponse.Role.ToLower() switch
                {
                    "student" => RedirectToAction("Dashboard", "Student"),
                    "parent" => RedirectToAction("Dashboard", "Parent"),
                    "teacher" => RedirectToAction("Dashboard", "Teacher"),
                    "admin" => RedirectToAction("Dashboard", "Admin"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка входа: {ex.Message}");
                ModelState.AddModelError("", "Ошибка сервера при входе");
                return View(model);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _supabase.Auth.SignOut();
            return RedirectToAction("Login", "Auth");
        }
    }
}