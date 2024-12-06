using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	public class PostsController : Controller
	{
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
		{
            var postari = from item in db.Posts
                           select item;
            ViewBag.Posts = postari;
            return View();
		}

		//public IActionResult Show(int id)
		//{
		//	Post post = db.Posts.Include("Tag")
		//				.Include("Comments").Include("User")
		//				.Include("Comments.User")
		//				.Where(art => art.Id == id)
		//			    .First();
		//	ViewBag.Post = post;
		//	return View();
		//}


	}
}
