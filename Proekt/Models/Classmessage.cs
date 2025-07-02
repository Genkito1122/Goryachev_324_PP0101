using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Proekt.Models
{
    [Table("class_messages")]
    public class ClassMessage : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("class_id")]
        public Guid ClassId { get; set; }

        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("is_announcement")]
        public bool IsAnnouncement { get; set; }

        [Column("discussion_id")]
        public Guid? DiscussionId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Reference(typeof(UserProfile))]
        public UserProfile Sender { get; set; }

        [Reference(typeof(Class))]
        public Class Class { get; set; }

    }
}
