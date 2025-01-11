using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	public class FollowsController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _env;
		private readonly RoleManager<IdentityRole> _roleManager;
		public FollowsController(
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
			return View();
		}

		[HttpPost]
		public IActionResult New(string? id)
		{
			var follow = new Follow
			{
				FollowerId = _userManager.GetUserId(User),
				FollowedId = id,
				Date = DateTime.Now
			};
			// vedem vizibilitatea profilului pentru FollowedId
			var followed = db.Users.Find(id);
			if (followed.Privacy == true)
			{
				follow.Accepted = false;
			}
			else
			{
				follow.Accepted = true;
			}

			db.Follows.Add(follow);
			db.SaveChanges();

			return Redirect("/Users/Details/" + id);
		}

		[HttpGet]
		public IActionResult Index(string? id)
		{
			var user = db.ApplicationUsers.Where(u => u.Id == id).FirstOrDefault();
			ViewBag.Privacy = user.Privacy;
			// afisam cererile de follow unde followedId = id
			ViewBag.CountToBeAccepted = db.Follows.Where(f => f.FollowedId == id && f.Accepted == false).Count();
			ViewBag.ToBeAccepted = db.Follows.Include(f => f.Follower)
											 .Where(f => f.FollowedId == id && f.Accepted == false)
											 .OrderByDescending(f => f.Date)
											 .ToList();
			ViewBag.CountAccepted = db.Follows.Where(f => f.FollowedId == id && f.Accepted == true).Count();
			ViewBag.Accepted = db.Follows.Include(f => f.Follower)
										 .Where(f => f.FollowedId == id && f.Accepted == true)
										 .OrderByDescending(f => f.Date)
										 .ToList();
			return View();
		}

		[HttpPost]
		public IActionResult Accept(string? id)
		{
			var userId = _userManager.GetUserId(User);
			Follow follow = db.Follows.Where(f => f.FollowerId == id && f.FollowedId == userId).FirstOrDefault();
			if(follow != null)
			{
				follow.Accepted = true;
				follow.Date = DateTime.Now;
				db.SaveChanges();
			}
			return Redirect("/Follows/Index/" + userId);
		}

		[HttpPost]
		public IActionResult Delete(string? id)
		{
			var userId = _userManager.GetUserId(User);
			Follow follow = db.Follows.Where(f => f.FollowerId == id && f.FollowedId == userId).FirstOrDefault();
			if (follow != null)
			{
				db.Remove(follow);
				db.SaveChanges();
			}
			return Redirect("/Follows/Index/" + userId);
		}

	}
}
