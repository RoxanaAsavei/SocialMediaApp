using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Net.NetworkInformation;

namespace SocialMediaApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
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
        [Authorize(Roles = "User,Moderator,Admin")]
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
        /*        [HttpPost]
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
                }*/
        [Authorize(Roles = "Moderator,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Group group, IFormFile? Image)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("UserId", "User must be logged in to create a post.");
                return View(group);
            }

            /*            group.UserId = _userManager.GetUserId(User);

                        if (string.IsNullOrEmpty(group.UserId))
                        {
                            ModelState.AddModelError("UserId", "Unable to determine the user ID.");
                            return View(group);
                        }*/

            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    return View(group);
                }

                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;

                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(group.Fotografie));
                group.Fotografie = databaseFileName;
            }
            if (TryValidateModel(group))
            {
                // Adăugare articol
                db.Groups.Add(group);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Groups");
            }
            return View(group);
        }
        [Authorize(Roles = "User,Moderator,Admin")]
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
            if (!(User.IsInRole("Moderator") || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            Group grup = db.Groups.Find(id);
            if (User.IsInRole("Moderator") || User.IsInRole("Admin"))
            {
                return View(grup);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        /* [HttpPost]
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
         }*/
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile? Image)
        {
            Group group = db.Groups.Find(id);
            if (!(User.IsInRole("Moderator") || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if(ModelState.IsValid)
            {
                group.Nume = requestGroup.Nume;
                group.Descriere = requestGroup.Descriere;
                group.Fotografie = requestGroup.Fotografie;

                if (Image != null && Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                    var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("PostImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                        return View(requestGroup);
                    }

                    var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                    var databaseFileName = "/images/" + Image.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }
                    ModelState.Remove(nameof(group.Fotografie));
                    group.Fotografie = databaseFileName;
                }

                db.SaveChanges();
                TempData["message"] = "Articolul a fost modificat";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestGroup);
            }
        }
    }
}