using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using Supabase;
using System.Security.Claims;

namespace Proekt.Controllers
{
    public class StudentController : Controller
    {
        private readonly Supabase.Client _supabase;

        public StudentController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Dashboard()
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var studentClasses = await _supabase
                .From<ClassMember>()
                .Where(cm => cm.UserId == studentId)
                .Get();

            var classIds = studentClasses.Models.Select(cm => cm.ClassId).Distinct().ToList();

            var allClasses = await _supabase
                .From<Class>()
                .Get();

            var classes = allClasses.Models
                .Where(c => classIds.Contains(c.Id))
                .ToList();

            return View(classes);
        }

        public IActionResult Chat(Guid classId)
        {
            return RedirectToAction("Chat", "Teacher", new { classId });
        }

        public IActionResult Discussions(Guid classId)
        {
            return RedirectToAction("List", "Discussion", new { classId });
        }
    }
}

