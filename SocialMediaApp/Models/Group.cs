using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Group
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Numele este obligatoriu")]
		public string Nume { get; set; }
		[Required(ErrorMessage = "Descrierea este obligatorie")]
		[MinLength(20, ErrorMessage = "Descrierea trebuie sa aiba minim 20 de caractere")]
		[MaxLength(500, ErrorMessage = "Descrierea nu poate avea mai mult de 500 de caractere")]
		public string Descriere { get; set; }
		public string? Fotografie { get; set; }
		// de vazut ce facem cu moderatorul care creeaza grupul
	}
}
