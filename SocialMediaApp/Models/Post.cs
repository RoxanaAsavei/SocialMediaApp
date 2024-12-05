using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Post
	{
        [Key]
        public int Id { get; set; }

        public string Continut { get; set; }

        public string Locatie { get; set; }

        public int? TagId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
