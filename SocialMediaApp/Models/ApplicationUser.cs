using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
	public class ApplicationUser : IdentityUser
	{
		public virtual ICollection<UserGroup>? UserGroups { get; set; }
		public virtual ICollection<Comment>? Comments { get; set; }
		public virtual ICollection<Tag>? Tags { get; set; }	
		public virtual ICollection<Post>? Posts { get; set; }
		public virtual ICollection<Follow>? Followers { get; set; }
		public virtual ICollection<Follow>? Following { get; set; }
		public virtual ICollection<Like>? Likes { get; set; }
		public virtual ICollection<Group>? ModeratedGroups { get; set; }

		[Required(ErrorMessage = "Adaugă o scurtă descriere.")]
		[StringLength(100, ErrorMessage = "Descrierea trebuie să aibă cel puțin {2} caractere.", MinimumLength = 5)]
		public string Description { get; set; }
		public string? Image { get; set; }

		[Required(ErrorMessage = "Prenumele este obligatoriu.")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Numele este obligatoriu.")]
		public string LastName { get; set; }


		public bool Privacy { get; set; } // true -> private account
							  // false -> public account


		// tinem minte rolurile 
		[NotMapped]
		public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
