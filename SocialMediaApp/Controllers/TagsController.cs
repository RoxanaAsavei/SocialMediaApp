using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	[Authorize(Roles = "Admin")]
	public class TagsController : Controller
	{
		
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public TagsController(
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
            var tags = db.Tags
							  .Include(t => t.Posts)
							  .Include(t => t.User)
							  .ToList();

            // Calculate the number of posts for each tag
            foreach (var tag in tags)
            {
                tag.PostCount = tag.Posts?.Count ?? 0;
            }
            ViewBag.Tags = tags;
			return View();
		}
		public ActionResult Show(int id)
		{
			var tag = db.Tags.Find(id);
			if (tag == null)
			{
				return NotFound();
			}
			var posts = db.Posts
								.Include(p => p.Tag)
								.Include(p => p.Comments)
								.Include(p => p.User)
								.Where(p => p.TagId == id)
								.ToList();
			ViewBag.Posts = posts;
			ViewBag.NoPosts = posts.Count();

			return View(tag);
		}


		public IActionResult New()
		{
			return View();
		}

        [HttpPost]
		public IActionResult New(Tag t)
		{
			
			try
			{
				t.UserId = _userManager.GetUserId(User);
				t.Data = DateTime.Now;
				db.Tags.Add(t);
				db.SaveChanges();
				TempData["message"] = "Tagul a fost adaugata";
				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				return View(t);
			}
		}
		public IActionResult Edit(int id)
		{
			Tag tag = db.Tags.Find(id);
			ViewBag.Tag = tag;
            if (tag.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(tag);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
		}
		[HttpPost]
		public ActionResult Edit(int id, Tag requestTag)
		{
			Tag tag = db.Tags.Find(id);
            if (!(tag.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if(ModelState.IsValid)
			{
				tag.Denumire = requestTag.Denumire;
				db.SaveChanges();
				TempData["message"] = "Categoria a fost modificata!";
				return RedirectToAction("Index");
			}
			else
			{
				return View(requestTag);
			}
		}
		public ActionResult Delete(int id)
		{
			Tag tag = db.Tags.Find(id);
            if (!(tag.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            db.Tags.Remove(tag);
			TempData["message"] = "Categoria a fost stearsa";
			 db.SaveChanges();
			return RedirectToAction("Index");
		}
	}
}