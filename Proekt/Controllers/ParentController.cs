using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using System.Security.Claims;

namespace Proekt.Controllers
{
    public class ParentController : Controller
    {
        private readonly Supabase.Client _supabase;

        public ParentController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Dashboard()
        {
            var parentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var links = await _supabase
                .From<StudentParentLink>()
                .Where(l => l.ParentId == parentId)
                .Get();

            var studentIds = links.Models.Select(l => l.StudentId).ToList();

            var allMembers = await _supabase
                .From<ClassMember>()
                .Get();

            var classMembers = allMembers.Models
                .Where(cm => studentIds.Contains(cm.UserId))
                .ToList();

            var classIds = classMembers.Select(cm => cm.ClassId).Distinct().ToList();

            var allClasses = await _supabase
                .From<Class>()
                .Get();

            var classes = allClasses.Models
                .Where(c => classIds.Contains(c.Id))
                .ToList();

            return View(classes);
        }

        [HttpGet]
        public IActionResult LinkStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LinkStudent(string studentEmail)
        {
            if (string.IsNullOrWhiteSpace(studentEmail))
            {
                TempData["Error"] = "Email обязателен";
                return RedirectToAction("LinkStudent");
            }

            var parentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);


            var student = await _supabase
                .From<UserProfile>()
                .Where(u => u.Email == studentEmail && u.Role == "student")
                .Single();

            if (student == null)
            {
                TempData["Error"] = "Ученик с таким email не найден";
                return RedirectToAction("LinkStudent");
            }

            var existingLink = await _supabase
                .From<StudentParentLink>()
                .Where(l => l.ParentId == parentId && l.StudentId == student.Id)
                .Single();

            if (existingLink != null)
            {
                TempData["Error"] = "Вы уже привязаны к этому ученику";
                return RedirectToAction("LinkStudent");
            }


            var link = new StudentParentLink
            {
                StudentId = student.Id,
                ParentId = parentId
            };

            await _supabase.From<StudentParentLink>().Insert(link);

            var studentClassMembers = await _supabase
                .From<ClassMember>()
                .Where(cm => cm.UserId == student.Id)
                .Get();

            foreach (var member in studentClassMembers.Models)
            {
               
                var existingParentInClass = await _supabase
                    .From<ClassMember>()
                    .Where(cm => cm.ClassId == member.ClassId && cm.UserId == parentId)
                    .Single();

                if (existingParentInClass == null)
                {
                    var parentMember = new ClassMember
                    {
                        ClassId = member.ClassId,
                        UserId = parentId,
                        RoleInClass = "parent"
                    };

                    await _supabase.From<ClassMember>().Insert(parentMember);
                }
            }

            TempData["Success"] = $"Ученик {student.FullName} успешно привязан, и вы добавлены в его классы!";
            return RedirectToAction("Dashboard");
        }
    }
}
