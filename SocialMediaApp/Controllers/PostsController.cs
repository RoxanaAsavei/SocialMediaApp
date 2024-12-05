using Microsoft.AspNetCore.Mvc;

namespace SocialMediaApp.Controllers
{
	public class PostsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
