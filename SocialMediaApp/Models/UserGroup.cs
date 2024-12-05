namespace SocialMediaApp.Models
{
	public class UserGroup
	{
		public int Id { get; set; }
		public string? UserId { get; set; }
		public int? GroupId { get; set; }
		public virtual ApplicationUser User { get; set; }
		public virtual Group Group { get; set; }

	}
}
