using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Join
    {
        public string? UserId { get; set; }
        public string? GroupId { get; set; }
        public bool? Accepted { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }

    }
}
