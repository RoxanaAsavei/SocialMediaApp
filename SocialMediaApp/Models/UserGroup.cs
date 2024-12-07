using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class UserGroup
	{
		[Key]
		public int Id { get; set; }
		public string? UserId { get; set; }
		public int? GroupId { get; set; }
		public virtual ApplicationUser User { get; set; }
		public virtual Group Group { get; set; }

	}
}
