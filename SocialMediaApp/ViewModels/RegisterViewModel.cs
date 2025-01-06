using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
	public class RegisterViewModel
	{

		[Required(ErrorMessage = "Prenumele este obligatoriu.")]
		public string FirstName { get; set; }
	
		[Required(ErrorMessage = "Numele este obligatoriu.")]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Selectează o fotografie de profil.")]
		public IFormFile Image { get; set; }

		[Required(ErrorMessage = "Emailul este obligatoriu.")]
		[EmailAddress(ErrorMessage = "Adresa de email nu este validă.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Adaugă o scurtă descriere.")]
		[StringLength(100, ErrorMessage = "Descrierea trebuie să aibă cel puțin {2} caractere.", MinimumLength = 5)]
		public string Description { get; set; }

		[Required(ErrorMessage = "Parola este obligatorie.")]
		[DataType(DataType.Password)]
		[StringLength(40, ErrorMessage = "Parola trebuie să aibă cel puțin {2} caractere.", MinimumLength = 8)]
		[Compare("ConfirmPassword", ErrorMessage = "Cele 2 parole nu se potrivesc.")]
		[Display(Name = "Parolă")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Confirmarea parolei este obligatorie.")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirmă parola")]
		public string ConfirmPassword { get; set; }

		public bool Privacy { get; set; } = false;

	}
}
