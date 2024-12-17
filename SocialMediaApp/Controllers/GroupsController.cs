using Ganss.Xss;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Join(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = _userManager.GetUserId(User);
            var userGroup = new UserGroup
            {
                UserId = userId,
                GroupId = id
            };
            db.UserGroups.Add(userGroup);
            await db.SaveChangesAsync();
            return Redirect("/Groups/Show/" + id);

        }

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

        public IActionResult Show(int id)
        {
            Group group = db.Groups.Find(id);
            return View(group);
        }

        [HttpPost]
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
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile Image)
        {
            Group group = db.Groups.Find(id);
            try
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
            catch (Exception)
            {
                return View(requestGroup);
            }
        }

        [HttpGet] // adaugarea postarii cu get -> formular
        public IActionResult AddPostToGroup()
        {
            Post post = new Post();
            return View(post);
        }

        [HttpPost] // adaugarea postarii cu post -> salvare in baza de date
        public async Task<IActionResult> New(Post post, IFormFile? Image, int id)
        {

			if (User.Identity == null || !User.Identity.IsAuthenticated)
			{
				ModelState.AddModelError("UserId", "User must be logged in to create a post.");
				post.Tags = GetAllTags();
				return View(post);
			}
			var sanitizer = new HtmlSanitizer();
			post.Data = DateTime.Now;
			post.NrComments = 0;
			post.UserId = _userManager.GetUserId(User);
            post.GroupId = id;

			if (string.IsNullOrEmpty(post.UserId))
			{
				ModelState.AddModelError("UserId", "Unable to determine the user ID.");
				post.Tags = GetAllTags();
				return View(post);
			}

			if (post.TagId == null)
			{
				ModelState.AddModelError("TagId", "Tag is required.");
				post.Tags = GetAllTags();
				return View(post);
			}

			if (Image != null && Image.Length > 0)
			{
				// Verificăm extensia
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
				var fileExtension = Path.GetExtension(Image.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("PostImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
					post.Tags = GetAllTags();
					return View(post);
				}

				// Cale stocare
				var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
				var databaseFileName = "/images/" + Image.FileName;

				// Salvare fișier
				using (var fileStream = new FileStream(storagePath, FileMode.Create))
				{
					await Image.CopyToAsync(fileStream);
				}
				ModelState.Remove(nameof(post.Image));
				post.Image = databaseFileName;
			}
			if (TryValidateModel(post))
			{
				post.Continut = sanitizer.Sanitize(post.Continut);
				// Adăugare postare
				db.Posts.Add(post);
				await db.SaveChangesAsync();
				return RedirectToAction("Index", "Posts");
			}
			post.Tags = GetAllTags();
			return View(post);
		}


		[NonAction]
		public IEnumerable<SelectListItem> GetAllTags()
		{
			var selectList = new List<SelectListItem>();
			var tags = from tag in db.Tags
					   select tag;
			// iteram prin taguri
			foreach (var tag in tags)
			{
				selectList.Add(new SelectListItem
				{
					Value = tag.Id.ToString(),
					Text = tag.Denumire
				});
			}

			return selectList;
		}

	}
}