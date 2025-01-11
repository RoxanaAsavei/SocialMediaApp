using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Join
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? GroupId { get; set; }
        public bool? Accepted { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }

    }
}
