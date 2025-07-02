using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proekt.Models
{
    [Table("classes")]
    public class Class : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("teacher_id")]
        public Guid? TeacherId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
