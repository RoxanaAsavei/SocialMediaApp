using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
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
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
		{
			if (TempData.ContainsKey("message"))
			{
				ViewBag.message = TempData["message"].ToString();
			}
/*			var tags = from tag in db.Tags
					   orderby tag.Denumire
					   select tag;*/
            var tags = db.Tags.Include(t => t.Posts).ToList();

            // Calculate the number of posts for each tag
            foreach (var tag in tags)
            {
                tag.PostCount = tag.Posts?.Count ?? 0;
            }
            ViewBag.Tags = tags;
			return View();
		}
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Show(int id)
		{
			/*Tag tag = db.Tags.Find(id);*/
			var tag = db.Tags.Include(t => t.Posts).ThenInclude(p => p.User).FirstOrDefault(t => t.Id == id);
			if (tag == null)
			{
				return NotFound();
			}
            ViewBag.Tag = tag;
            ViewBag.Posts = tag.Posts;
            return View(tag);
		}

		[Authorize(Roles = "Moderator,Admin")]
		public IActionResult New()
		{
			return View();
		}
        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
		public IActionResult New(Tag t)
		{
			t.UserId = _userManager.GetUserId(User);
			try
			{
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