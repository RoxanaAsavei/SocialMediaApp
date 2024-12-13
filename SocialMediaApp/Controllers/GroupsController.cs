using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
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
            var grupuri = from item in db.Groups
                          select item;
            ViewBag.Groups = grupuri;
            return View();
        }
        [HttpGet]
        public IActionResult New()
        {
            Group group = new Group();
            return View(group);
        }
        [HttpPost]
        public IActionResult New(Group group)
        {
            if(ModelState.IsValid)
            {
                db.Groups.Add(group);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost creat";
                return RedirectToAction("Index");
            }
            else
            {
                return View(group);
            }
        }
        public IActionResult Show(int id)
        {
            Group group = db.Groups.Include("Posts")
                            .Where(group => group.Id == id)
                            .First();
            return View(group);
        }
        public IActionResult Delete(int id)
        {
            var group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            Group grup = db.Groups.Find(id);
            return View(grup);
        }

        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            Group group = db.Groups.Find(id);

            if (ModelState.IsValid)
            {
                group.Nume = requestGroup.Nume;
                group.Descriere = requestGroup.Descriere;
                group.Fotografie = requestGroup.Fotografie;
                TempData["message"] = "Articolul a fost modificat";
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            else
            {
                return View(requestGroup);
            }
        }
    }
}