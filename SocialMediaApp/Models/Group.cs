using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
	public class Group
	{
		[Key]
		public int GroupId { get; set; }
		public string Nume { get; set; }
		public string Descriere { get; set; }
		public string? Fotografie { get; set; }
		// de vazut ce facem cu moderatorul care creeaza grupul
	}
}
