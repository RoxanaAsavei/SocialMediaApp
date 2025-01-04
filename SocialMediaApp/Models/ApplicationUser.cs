using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
	public class ApplicationUser : IdentityUser
	{
		public virtual ICollection<UserGroup>? UserGroups { get; set; }
		public virtual ICollection<Comment>? Comments { get; set; }
		public virtual ICollection<Tag>? Tags { get; set; }	
		public virtual ICollection<Post>? Posts { get; set; }
		public string? Description { get; set; }
		public string? Image { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }

		public bool Privacy { get; set; } // true -> private account
							  // false -> public account

		public virtual ICollection<Follow>? Followers { get; set; }
		public virtual ICollection<Follow>? Following { get; set; }

		// tinem minte rolurile 
		[NotMapped]
		public IEnumerable<SelectListItem>? AllRoles { get; set; }


	}
}
