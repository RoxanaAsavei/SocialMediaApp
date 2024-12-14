using Microsoft.AspNetCore.Mvc;

namespace SocialMediaApp.Controllers
{
	public class UsersController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
