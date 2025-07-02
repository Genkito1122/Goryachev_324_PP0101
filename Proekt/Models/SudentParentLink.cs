using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proekt.Models
{
    [Table("student_parent_links")]
    public class StudentParentLink : BaseModel
    {
        [Column("student_id")]
        public Guid StudentId { get; set; }

        [Column("parent_id")]
        public Guid ParentId { get; set; }
    }
}
