using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
	public class UserGroup
	{
		[Key, Column(Order = 0)]
		public string UserId { get; set; }

		[Key, Column(Order = 1)]
		public int GroupId { get; set; }

		public virtual ApplicationUser User { get; set; }
		public virtual Group Group { get; set; }
	}
}