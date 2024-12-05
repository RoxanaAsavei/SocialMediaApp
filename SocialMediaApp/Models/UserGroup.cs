namespace SocialMediaApp.Models
{
	public class UserGroup
	{
		public int Id { get; set; }
		/// <summary>
		/// public string? UserId { get; set; }
		/// </summary>
		public int? GroupId { get; set; }
		//public virtual ApplicationUser User { get; set; }
		public virtual Group Group { get; set; }

	}
}
