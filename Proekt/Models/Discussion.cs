using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Proekt.Models
{
    [Table("discussions")]
    public class Discussion : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("class_id")]
        public Guid ClassId { get; set; }

        [Column("creator_id")]
        public Guid CreatorId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    [Table("discussion_comments")]
    public class DiscussionComment : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("discussion_id")]
        public Guid DiscussionId { get; set; }

        [Column("author_id")]
        public Guid AuthorId { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Reference(typeof(UserProfile))]
        public UserProfile Sender { get; set; }
    }

    public class DiscussionViewModel
    {
        public Discussion Discussion { get; set; }
        public List<DiscussionComment> Comments { get; set; }
        public string NewComment { get; set; }
    }
}
