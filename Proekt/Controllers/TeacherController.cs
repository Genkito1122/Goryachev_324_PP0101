using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest;
using Supabase.Postgrest.Responses;
using Supabase.Postgrest.Responses;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proekt.Controllers
{
    public class TeacherController : Controller
    {
        private readonly Supabase.Client _supabase;

        public TeacherController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Dashboard()
        {
            var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherIdStr))
                return RedirectToAction("Login", "Auth");

            var teacherId = Guid.Parse(teacherIdStr);

            var classes = await _supabase
                .From<Class>()
                .Where(c => c.TeacherId == teacherId)
                .Get();

            return View(classes.Models);
        }
        public async Task<IActionResult> Chat(Guid classId)
        {
            var messages = await _supabase
                .From<ClassMessage>()
                .Select("*, sender:users(*)")
                .Where(m => m.ClassId == classId)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var members = await _supabase
                .From<ClassMember>()
                .Select("*, user:users(*)")
                .Where(m => m.ClassId == classId)
                .Get();

            var classData = await _supabase
                .From<Class>()
                .Where(c => c.Id == classId)
                .Single();

            return View(new ClassChatViewModel
            {
                ClassId = classId,
                ClassName = classData?.Name,
                Messages = messages.Models,
                Members = members.Models
            });
        }

        public async Task<IActionResult> Direct(Guid userId)
        {
            var currentUserId = Guid.Parse(User.Identity.Name);

            var messages = await _supabase
                .From<Message>()
                .Where(m =>
                    (m.SenderId == currentUserId && m.RecipientId == userId) ||
                    (m.SenderId == userId && m.RecipientId == currentUserId))
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return View("Direct", messages.Models);
        }
        [HttpGet]
        public IActionResult CreateClass()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(string className)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(className))
                {
                    TempData["Error"] = "Название класса обязательно";
                    return RedirectToAction("CreateClass");
                }
                var teacherId = await GetCurrentTeacherId();
                if (teacherId == Guid.Empty)
                {
                    TempData["Error"] = "Преподаватель не авторизован";
                    return RedirectToAction("Login", "Auth");
                }
                var teacherExists = await _supabase
                    .From<UserProfile>()
                    .Where(u => u.Id == teacherId && u.Role == "teacher")
                    .Single();

                if (teacherExists == null)
                {
                    TempData["Error"] = "Преподаватель не найден или не имеет нужной роли";
                    return RedirectToAction("CreateClass");
                }

                var newClass = new Class
                {
                    Name = className.Trim(),
                    TeacherId = teacherId,
                    CreatedAt = DateTime.UtcNow
                };

                Debug.WriteLine($"Попытка создать класс: {newClass.Name}, TeacherID: {newClass.TeacherId}");

                var response = await _supabase
                    .From<Class>()
                    .Insert(newClass, new QueryOptions { Returning = QueryOptions.ReturnType.Representation });

                if (response.Models == null || !response.Models.Any())
                {
                    throw new Exception("Не удалось создать класс: ответ от сервера пустой");
                }

                var createdClass = response.Models.First();
                Debug.WriteLine($"Создан класс ID: {createdClass.Id}");

                var classMember = new ClassMember
                {
                    ClassId = createdClass.Id,
                    UserId = teacherId,
                    RoleInClass = "teacher",
                };

                var memberResponse = await _supabase
                    .From<ClassMember>()
                    .Insert(classMember);

                TempData["Success"] = $"Класс {className} успешно создан!";
                return RedirectToAction("CreateClass");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex}");
                TempData["Error"] = $"Ошибка при создании класса: {ex.Message}";
                return RedirectToAction("CreateClass");
            }
        }

        private async Task<Guid> GetCurrentTeacherId()
        {
            try
            {
                var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdStr))
                    return Guid.Empty;

                if (!Guid.TryParse(teacherIdStr, out var teacherId))
                {
                    var user = await _supabase.From<UserProfile>()
                        .Where(u => u.Email == User.Identity.Name)
                        .Single();

                    return user?.Id ?? Guid.Empty;
                }

                return teacherId;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(Guid classId, string email)
        {
            try
            {
                var user = await _supabase
                    .From<UserProfile>()
                    .Where(u => u.Email == email)
                    .Single();

                if (user == null)
                {
                    TempData["Error"] = "Пользователь с таким email не найден.";
                    return RedirectToAction("Chat", new { classId });
                }

                if (user.Role == "teacher")
                {
                    TempData["Error"] = "Нельзя добавить другого преподавателя в этот класс.";
                    return RedirectToAction("Chat", new { classId });
                }

                var exists = await _supabase
                    .From<ClassMember>()
                    .Where(m => m.ClassId == classId && m.UserId == user.Id)
                    .Single();

                if (exists != null)
                {
                    TempData["Error"] = "Пользователь уже добавлен в этот класс.";
                    return RedirectToAction("Chat", new { classId });
                }

                var newMember = new ClassMember
                {
                    ClassId = classId,
                    UserId = user.Id,
                    RoleInClass = user.Role // Используем реальную роль из профиля
                };

                await _supabase
                    .From<ClassMember>()
                    .Insert(newMember);

                TempData["Success"] = $"Пользователь {user.FullName} добавлен как {user.Role}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ошибка при добавлении участника: " + ex.Message;
            }

            return RedirectToAction("Chat", new { classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember([FromBody] RemoveMemberRequest request)
        {
            try
            {
                var teacherId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var isTeacher = await _supabase
                    .From<ClassMember>()
                    .Where(m => m.ClassId == request.ClassId)
                    .Where(m => m.UserId == teacherId)
                    .Where(m => m.RoleInClass == "teacher")
                    .Single();

                if (isTeacher == null)
                    return Json(new { success = false, error = "Только учитель может удалять участников." });

                // Удаляем участника
                await _supabase
                    .From<ClassMember>()
                    .Where(m => m.ClassId == request.ClassId && m.UserId == request.UserId)
                    .Delete();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public class RemoveMemberRequest
        {
            public Guid ClassId { get; set; }
            public Guid UserId { get; set; }
        } 

    }
}
