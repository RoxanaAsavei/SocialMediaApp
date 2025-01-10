using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Controllers
{
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly IWebHostEnvironment _env;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signManager;
		private readonly RoleManager<IdentityRole> _roleManager;
	
		public AccountController(
		ApplicationDbContext context,
		IWebHostEnvironment env,
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		RoleManager<IdentityRole> roleManager
		)
		{
			db = context;
			_env = env;
			_userManager = userManager;
			_signManager = signInManager;
			_roleManager = roleManager;
		}
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _signManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Posts");
				}
				else
				{
					ModelState.AddModelError("", "Email sau parolă greșită.");
					return View(model);
				}
			}

			return View(model);

		}
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if(ModelState.IsValid)
			{
				// pregatim stringul pentru imagine
				// Verificăm extensia
				IFormFile Image = model.Image;
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
				var fileExtension = Path.GetExtension(Image.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("Image", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
					return View(model);
				}

				// Cale stocare
				var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
				var databaseFileName = "/images/" + Image.FileName;

				// Salvare fișier
				using (var fileStream = new FileStream(storagePath, FileMode.Create))
				{
					await Image.CopyToAsync(fileStream);
				}

				ModelState.Remove(nameof(model.Image));

				ApplicationUser user = new ApplicationUser
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					UserName = model.Email,
					Image = databaseFileName,
					Description = model.Description,
					Privacy = model.Privacy
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				
				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(user, "User");
					return RedirectToAction("Login", "Account");
				}
				else
				{
					foreach(var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}

					return View(model);
				}

			}
			return View(model);
		}

		public IActionResult VerifyEmail()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
		{
			if(ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					ModelState.AddModelError("", "Email invalid.");
					return View(model);
				}
				else
				{
					return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
				}

			}

			return View(model);
		}


		public IActionResult ChangePassword(string username)
		{
			if (string.IsNullOrEmpty(username))
			{
				return RedirectToAction("VerifyEmail", "Account");
			}

			return View(new ChangePasswordViewModel { Email = username});

		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if(ModelState.IsValid)
			{    
				var user = await _userManager.FindByNameAsync(model.Email);
				if(user != null)
				{
	
					var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
					if (result.Succeeded)
					{
						return RedirectToAction("Login", "Account");
					}

					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}

						return View(model);
					}

				}
				else
				{
					ModelState.AddModelError("", "Utilizatorul nu a fost găsit.");
					return View(model);
				}

			}
			else
			{
				ModelState.AddModelError("", "Datele introduse nu sunt valide.");
				return View(model);
			}
		}

		public async Task<IActionResult> Logout()
		{
			await _signManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}

	
}
