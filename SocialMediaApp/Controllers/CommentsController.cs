using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	public class CommentsController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		public CommentsController(
		ApplicationDbContext context,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager
		)
		{
			db = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		[HttpPost]
		public IActionResult Delete(int id)
		{
			Comment comm = db.Comments.Find(id);
			db.Comments.Remove(comm);
			db.SaveChanges();
			return Redirect("/Posts/Show/" + comm.PostId);
		}
		public IActionResult Edit(int id)
		{
			Comment comm = db.Comments.Find(id);
			return View(comm);
		}

		[HttpPost]
		public IActionResult Edit(int id, Comment requestComment)
		{
			Comment comm = db.Comments.Find(id);
			try
			{
				comm.Continut = requestComment.Continut;
				db.SaveChanges();
				return Redirect("/Posts/Show/" + comm.PostId);
			}
			catch (Exception)
			{
				return View(requestComment);
			}
		}
	}
}