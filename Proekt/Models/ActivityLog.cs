using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proekt.Models
{
    [Table("activity_logs")]
    public class ActivityLog : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("action")]
        public string Action { get; set; }  // "login", "message_sent", etc.

        [Column("details")]
        public Dictionary<string, object> Details { get; set; }  // JSONB в PostgreSQL

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
