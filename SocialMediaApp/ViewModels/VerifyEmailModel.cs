using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
	public class VerifyEmailModel
	{
		[Required(ErrorMessage = "Emailul este obligatoriu.")]
		[EmailAddress(ErrorMessage = "Adresa de email nu este validă.")]
		public string Email { get; set; }
	}
}
