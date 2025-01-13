using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
	public class Tag
	{
		[Key]
		public int Id { get; set; }

        [Required(ErrorMessage = "Denumirea este obligatorie")]
        [StringLength(100, ErrorMessage = "Denumirea nu poate avea mai mult de 100 de caractere")]
		[MinLength(3, ErrorMessage = "Denumirea trebuie sa aibă mai mult de 3 caractere")]
        public string Denumire { get; set; }
		public DateTime Data { get; set; }
		public virtual ICollection<Post>? Posts { get; set; }
        [Required(ErrorMessage = "User-ul este obligatorie")]
        public string? UserId { get; set; }	
		public virtual ApplicationUser? User { get; set; }
        [NotMapped]
        public int PostCount { get; set; }

    }
}
