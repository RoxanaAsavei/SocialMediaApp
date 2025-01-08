namespace SocialMediaApp.Models
{
	public class Like
	{
		public int? PostId { get; set; }
		public string? UserId { get; set; }
		virtual public ApplicationUser? User { get; set; }
		virtual public Post? Post { get; set; }
	}
}
