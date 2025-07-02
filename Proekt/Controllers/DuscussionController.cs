using Microsoft.AspNetCore.Mvc;
using Proekt.Models;
using Supabase;
using System.Security.Claims;

namespace Proekt.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly Supabase.Client _supabase;

        public DiscussionController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> List(Guid classId)
        {
            var discussions = await _supabase
                .From<Discussion>()
                .Where(d => d.ClassId == classId)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            ViewBag.ClassId = classId;
            return View(discussions.Models);
        }

        [HttpGet]
        public IActionResult Create(Guid classId)
        {
            ViewBag.ClassId = classId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid classId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Тема обязательна";
                return RedirectToAction("Create", new { classId });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var discussion = new Discussion
            {
                Id = Guid.NewGuid(),
                ClassId = classId,
                CreatorId = userId,
                Title = title.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _supabase.From<Discussion>().Insert(discussion);

            return RedirectToAction("List", new { classId });
        }

        public async Task<IActionResult> View(Guid id)
        {
            var discussion = await _supabase.From<Discussion>().Where(d => d.Id == id).Single();
            if (discussion == null) return NotFound();

            var comments = await _supabase
                .From<DiscussionComment>()
                .Select("*, sender:users(*)")
                .Where(c => c.DiscussionId == id)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return View(new DiscussionViewModel
            {
                Discussion = discussion,
                Comments = comments.Models
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(Guid discussionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("View", new { id = discussionId });

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var comment = new DiscussionComment
            {
                Id = Guid.NewGuid(),
                DiscussionId = discussionId,
                AuthorId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            await _supabase.From<DiscussionComment>().Insert(comment);
            return RedirectToAction("View", new { id = discussionId });
        }
    }
}
