using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
	public class Post
	{
        [Key]
        public int Id { get; set; }

        public string Continut { get; set; }

        public string Locatie { get; set; }

        public DateTime Data { get; set; }

        public int? TagId { get; set; }

        public string? UserId { get; set; }

        public virtual Tag Tag { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
		public IEnumerable<SelectListItem>? Tags { get; set; }
	}
}
