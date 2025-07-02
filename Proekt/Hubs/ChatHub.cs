using Microsoft.AspNetCore.SignalR;
using Proekt.Models;
using Supabase;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace Proekt.Hubs
{
    public class ChatHub : Hub
    {
        private readonly Supabase.Client _supabase;

        public ChatHub(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task SendClassMessage(Guid classId, Guid senderId, string content, bool isAnnouncement)
        {
            var message = new ClassMessage
            {
                Id = Guid.NewGuid(),
                ClassId = classId,
                SenderId = senderId,
                Content = content,
                IsAnnouncement = isAnnouncement,
                CreatedAt = DateTime.UtcNow
            };

            await _supabase.From<ClassMessage>().Insert(message);

            var sender = await _supabase.From<UserProfile>()
                .Where(u => u.Id == senderId)
                .Single();

            await Clients.Group(classId.ToString()).SendAsync("ReceiveClassMessage", new
            {
                Sender = new { sender.Id, sender.FullName },
                Content = content,
                IsAnnouncement = isAnnouncement,
                CreatedAt = message.CreatedAt
            });
            var members = await _supabase
                .From<ClassMember>()
                .Where(m => m.ClassId == classId && m.UserId != senderId)
                .Get();
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var classId = httpContext.Request.Query["classId"];

            if (!string.IsNullOrEmpty(classId))
                await Groups.AddToGroupAsync(Context.ConnectionId, classId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            var classId = httpContext.Request.Query["classId"];

            if (!string.IsNullOrEmpty(classId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, classId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}



