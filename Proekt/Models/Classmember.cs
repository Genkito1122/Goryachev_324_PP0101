using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proekt.Models
{
    [Table("class_members")]
    public class ClassMember : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("class_id")]
        public Guid ClassId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("role_in_class")]
        public string RoleInClass { get; set; }  

        [Reference(typeof(UserProfile))]
        public UserProfile User { get; set; }

        [Reference(typeof(Class))]
        public Class Class { get; set; }

    }
}
