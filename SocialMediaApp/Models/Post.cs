using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace SocialMediaApp.Models
{
	public class Post
	{
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Continut { get; set; }

		[StringLength(50, ErrorMessage = "Locatia nu poate avea mai mult de 50 de caractere")]
		public string Locatie { get; set; }

		public int NrComments { get; set; }
		public string? Image { get; set; }
		public string? Video { get; set; }

		public DateTime Data { get; set; }

        public int? GroupId { get; set; }
		public int? TagId { get; set; }

        public string? UserId { get; set; }

		public int NrLikes { get; set; }

		public virtual Tag? Tag { get; set; }
		public virtual Group? Group { get; set; }

		public virtual ApplicationUser? User { get; set; }

		public virtual ICollection<Like>? Likes { get; set; }
		public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
		public IEnumerable<SelectListItem>? Tags { get; set; }
	}
}
