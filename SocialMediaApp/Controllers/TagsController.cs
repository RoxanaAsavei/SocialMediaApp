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
    }
}
