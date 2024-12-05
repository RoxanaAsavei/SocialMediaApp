using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;

namespace SocialMediaApp.Controllers
{
	public class PostsController : Controller
	{
        private readonly ApplicationDbContext _db;
        public PostsController(ApplicationDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
		{
            var articles = from item in _db.Posts
                           select item;
            ViewBag.Articole = articles;
            return View();
		}
	}
}
