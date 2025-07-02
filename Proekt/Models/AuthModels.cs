using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Proekt.Models
{

    public class RegisterModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [RegularExpression("student|parent|teacher")]
        public string Role { get; set; }
    }

    public class LoginModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
    public class ClassChatViewModel
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } 
        public List<ClassMessage> Messages { get; set; }
        public List<ClassMember> Members { get; set; }
        public List<Discussion> Discussions { get; set; }
    }

}
