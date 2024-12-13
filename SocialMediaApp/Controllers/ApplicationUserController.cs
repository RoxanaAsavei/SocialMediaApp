using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Rules;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	public class ApplicationUserController : Controller
	{

		private readonly ApplicationDbContext db;
		private readonly IWebHostEnvironment _env;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		public ApplicationUserController(
		ApplicationDbContext context,
		IWebHostEnvironment env,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager
		)
		{
			db = context;
			_env = env;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public IActionResult Index()
		{
			var utilizatori = from item in db.Users
							  select item;	
			ViewBag.Users = utilizatori;
			return View();
		}


		// new cu get - afisarea formularului
		public IActionResult New()
		{
			return View();
		}	

	
	}
}
