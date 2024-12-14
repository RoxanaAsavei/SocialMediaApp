using Microsoft.AspNetCore.Identity;

namespace SocialMediaApp.Models
{
	public class ApplicationUser : IdentityUser
	{
		public virtual ICollection<Post>? Posts { get; set; }
		public virtual ICollection<Comment>? Comments { get; set; }
		public virtual ICollection<Tag>? Tags { get; set; }	

		public string? Description { get; set; }
		public string? Image { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }


	}
}
