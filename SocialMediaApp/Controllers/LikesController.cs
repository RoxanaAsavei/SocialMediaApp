using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	public class LikesController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		public LikesController(
		ApplicationDbContext context,
		IWebHostEnvironment env,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager
		)
		{
			db = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[HttpPost]
		public IActionResult Click(int id)
		{
			// luam id-ul userului care a dat like
			var userId = _userManager.GetUserId(User);
			// selectam postarea cu id-ul dat
			var post = db.Posts.FirstOrDefault(p => p.Id == id);
			// daca userul a dat deja like, se pune unlike
			var like = db.Likes.FirstOrDefault(l => l.UserId == userId && l.PostId == id);
			if (like == null)
			{

				like = new Like
				{
					UserId = userId,
					PostId = id
				};
				db.Likes.Add(like);
				// crestem numarul de like-uri al postarii
				post.NrLikes += 1;
			}
			else
			{
				db.Likes.Remove(like);
				post.NrLikes -= 1;
			}
			db.SaveChanges();
			return RedirectToAction("Index", "Posts");
		}


		[HttpGet]
		public IActionResult ShowLikes(int id)
		{
			// luam toti userii care au dat like la postare
			var users = db.ApplicationUsers
						  .Where(u => u.Likes.Any(l => l.PostId == id))
						  .Include(u => u.Likes)
						  .ToList();
			ViewBag.Users = users;
			ViewBag.UsersCt = users.Count();
			return View();
		}

	}
}
