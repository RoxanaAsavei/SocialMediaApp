using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Follow
	{
		public string? FollowerId { get; set; }
		public string? FollowedId { get; set; }
		public DateTime? Date { get; set; }
		public bool? Accepted { get; set; }
		public virtual ApplicationUser? Follower { get; set; }
		public virtual ApplicationUser? Followed { get; set; }

	}
}
