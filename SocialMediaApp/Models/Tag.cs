using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Tag
	{
		[Key]
		public int Id { get; set; }
		public string Denumire { get; set; }
		public DateTime Data { get; set; }
		public virtual ICollection<Post> Posts { get; set; }
		public string? UserId { get; set; }	
		public virtual ApplicationUser? User { get; set; }	

	}
}
