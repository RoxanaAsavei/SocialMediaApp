using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Comment
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Continutul este obligatoriu")]
		[MaxLength(500, ErrorMessage = "Continutul nu poate avea mai mult de 500 de caractere")]	
		public string Continut { get; set; }

		public int? PostId { get; set; }
		public string? UserId { get; set; }
		public DateTime Data { get; set; }
		public virtual Post? Post { get; set; }
		public virtual ApplicationUser? User { get; set; }

	}
}