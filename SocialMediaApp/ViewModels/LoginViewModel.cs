using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage="Emailul este obligatoriu.")]
		[EmailAddress(ErrorMessage = "Adresa de email nu este validă.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Parola este obligatorie.")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Ține-mă minte")]
		public bool RememberMe { get; set; }
	}
}
