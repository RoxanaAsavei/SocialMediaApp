using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		public IActionResult Index()
		{
			if (TempData.ContainsKey("message"))
			{
				ViewBag.message = TempData["message"].ToString();
			}
			var tags = from tag in db.Tags
					   orderby tag.Denumire
					   select tag;
			ViewBag.Tags = tags;
			return View();
		}
		public ActionResult Show(int id)
		{
			Tag tag = db.Tags.Find(id);
			ViewBag.Tag = tag;
			return View();
		}
		public IActionResult New()
		{
			return View();
		}
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
			return View(tag);
		}
		[HttpPost]
		public ActionResult Edit(int id, Tag requestTag)
		{
			Tag tag = db.Tags.Find(id);
			try
			{
				tag.Denumire = requestTag.Denumire;
				db.SaveChanges();
				TempData["message"] = "Categoria a fost modificata!";
				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				return View(requestTag);
			}
		}
		public ActionResult Delete(int id)
		{
			Tag tag = db.Tags.Find(id);
			db.Tags.Remove(tag);
			TempData["message"] = "Categoria a fost stearsa";
			 db.SaveChanges();
			return RedirectToAction("Index");
		}
	}
}