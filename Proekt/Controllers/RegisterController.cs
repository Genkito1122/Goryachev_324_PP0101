using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using System;
using System.Threading.Tasks;

namespace Proekt.Controllers
{

    public class RegisterController : Controller
    {
        private readonly Supabase.Client _supabase;

        public RegisterController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // 1. Регистрация пользователя
                var session = await _supabase.Auth.SignUp(model.Email, model.Password);

                if (session?.User == null)
                {
                    ModelState.AddModelError("", "Ошибка регистрации");
                    return View(model);
                }

                var userProfile = new UserProfile
                {
                    Id = Guid.Parse(session.User.Id),
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = model.Role.ToLower()
                };

                var response = await _supabase
                    .From<UserProfile>()
                    .Insert(userProfile);

                if (response.ResponseMessage?.IsSuccessStatusCode != true)
                {
                    ModelState.AddModelError("", "Ошибка сохранения профиля. Попробуйте зарегистрироваться снова.");
                    return View(model);
                }

                TempData["RegistrationSuccess"] = "Регистрация прошла успешно! Пожалуйста, войдите в систему.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
                return View(model);
            }
        }
    }
}