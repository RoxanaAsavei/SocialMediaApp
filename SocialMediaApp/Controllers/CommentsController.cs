using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
			// scadem numarul de comentarii de la post
			Post post = db.Posts.Find(comm.PostId);
            if (!(comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            post.NrComments--;
			db.Comments.Remove(comm);
			db.SaveChanges();
			return Redirect("/Posts/Show/" + comm.PostId);
		}
		public IActionResult Edit(int id)
		{
			Comment comm = db.Comments.Find(id);
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
		}

		[HttpPost]
		public IActionResult Edit(int id, Comment requestComment)
		{
			Comment comm = db.Comments.Find(id);
            if (!(comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if(ModelState.IsValid)
			{
				comm.Continut = requestComment.Continut;
				db.SaveChanges();
				return Redirect("/Posts/Show/" + comm.PostId);
			}
			else
			{
				return View(requestComment);
			}
		}
	}
}