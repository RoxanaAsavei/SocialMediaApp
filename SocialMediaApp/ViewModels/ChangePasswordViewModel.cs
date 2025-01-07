using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Emailul este obligatoriu.")]
		[EmailAddress(ErrorMessage = "Adresa de email nu este validă.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Introduceți parola actuală.")]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; }

		[Required(ErrorMessage = "Parola este obligatorie.")]
		[DataType(DataType.Password)]
		[StringLength(40, ErrorMessage = "Parola trebuie să aibă cel puțin {2} caractere.", MinimumLength = 8)]
		[Compare("ConfirmNewPassword", ErrorMessage = "Cele 2 parole nu se potrivesc.")]
		[Display(Name = "Parolă nouă")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Confirmarea parolei este obligatorie.")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirmă noua parolă")]
		public string ConfirmNewPassword { get; set; }
	}
}
