using Ganss.Xss;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index()
        {
            var grupuri = db.Groups.ToList();
            ViewBag.Groups = grupuri;
			return View();
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            var existingRequest = db.Joins.FirstOrDefault(j => j.UserId == userId && j.GroupId == id);
            if (existingRequest == null)
            {
                var joinRequest = new Join
                {
                    UserId = userId,
                    GroupId = id,
                    Accepted = false
                };
                db.Joins.Add(joinRequest);
                await db.SaveChangesAsync();
            }
			return RedirectToAction("Show", "Groups", new { id = id });

		}

		[Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> JoinRequests(int groupId)
        {
            var joinRequests = await db.Joins
                .Include(j => j.User)
                .Where(j => j.GroupId == groupId && j.Accepted == false)
                .ToListAsync();

            var group = db.Groups.Where(g => g.Id == groupId).FirstOrDefault();
            ViewBag.GroupId = groupId;
            ViewBag.GroupName = group.Nume;
            return View(joinRequests);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptJoinRequest(int joinId)
        {
            var joinRequest = await db.Joins.FindAsync(joinId);
            if (joinRequest != null)
            {
                joinRequest.Accepted = true;
                var userGroup = new UserGroup
                {
                    UserId = joinRequest.UserId,
                    GroupId = joinRequest.GroupId ?? 0
                };
                db.UserGroups.Add(userGroup);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("JoinRequests", new { groupId = joinRequest.GroupId });
        }

        [HttpPost]
        public async Task<IActionResult> DeclineJoinRequest(int joinId)
        {
            var joinRequest = await db.Joins.FindAsync(joinId);
            if (joinRequest != null)
            {
                db.Joins.Remove(joinRequest);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("JoinRequests", new { groupId = joinRequest.GroupId });
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);
            var isModerator = (db.Groups.Where(g => g.Id == id && g.ModeratorId == userId).FirstOrDefault() != null);
            if (isModerator)
            {
                TempData["ErrorMessage"] = "Moderators cannot leave the group.";
                return RedirectToAction("Index");
            }

            var existingMembership = db.UserGroups.FirstOrDefault(ug => ug.UserId == userId && ug.GroupId == id);
            if (existingMembership != null)
            {
                db.UserGroups.Remove(existingMembership);
            }

            var existingJoinRequest = db.Joins.FirstOrDefault(j => j.UserId == userId && j.GroupId == id);
            if (existingJoinRequest != null)
            {
                db.Joins.Remove(existingJoinRequest);
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(int groupId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isModerator = db.Groups.Any(g => g.Id == groupId && g.ModeratorId == currentUserId);
			var isAdmin = User.IsInRole("Admin");

            if (!isModerator && !isAdmin)
            {
                return Forbid();
            }

            var userGroup = await db.UserGroups.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
            if (userGroup != null)
            {
                db.UserGroups.Remove(userGroup);
            }

            var joinRequest = await db.Joins.FirstOrDefaultAsync(j => j.UserId == userId && j.GroupId == groupId);
            if (joinRequest != null)
            {
                db.Joins.Remove(joinRequest);
            }

            await db.SaveChangesAsync();

            return RedirectToAction("UsersInGroup", new { id = groupId });
        }

        public async Task<IActionResult> UsersInGroup(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            var group = await db.Groups
                                .Include(g => g.UserGroups)
                                .ThenInclude(ug => ug.User)
                                .FirstOrDefaultAsync(g => g.Id == id);
            var moderatedGroups = db.Groups.Where(g => g.ModeratorId == userId).Select(g => g.Id).ToList();
            ViewBag.UserId = userId;
            ViewBag.ModeratedGroups = moderatedGroups;
            ViewBag.UserRoles = userRoles;
            return View(group);
        }



        [HttpGet]
        public IActionResult New()
        {
            Group group = new Group();
            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> New(Group group, IFormFile? Fotografie)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("UserId", "User must be logged in to create a post.");
                return View(group);
            }
            if (Fotografie != null && Fotografie.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Fotografie.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    return View(group);
                }
                var storagePath = Path.Combine(_env.WebRootPath, "images", Fotografie.FileName);
                var databaseFileName = "/images/" + Fotografie.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Fotografie.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(group.Fotografie));
                group.Fotografie = databaseFileName;
            }
            if (TryValidateModel(group))
            {
                var userId = _userManager.GetUserId(User);
                group.ModeratorId = userId;
                db.Groups.Add(group);
                db.SaveChanges();
                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = group.Id
                };
                db.UserGroups.Add(userGroup);
                db.SaveChanges();
                return RedirectToAction("Index", "Groups");
            }
            return View(group);
        }


        public IActionResult Show(int id)
        {
            var group = db.Groups
                                .Include("UserGroups")
                                .Include("Posts")
                                .Include("Posts.User")
                                .Include("Posts.Tag")
								.Where(g => g.Id == id)
                                .FirstOrDefault();

            if (group == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            ViewBag.userId = userId;
            ViewBag.CanDeleteGroup = isAdmin || (userId == group.ModeratorId);
            ViewBag.IsMember = (db.UserGroups
                                            .Where(ug => ug.UserId == userId && ug.GroupId == id)
                                            .FirstOrDefault()) != null;
            ViewBag.JoinRequest = (db.Joins
                                          .Where(j => j.UserId == userId && j.GroupId == id && j.Accepted == false)
                                          .FirstOrDefault()) != null;
            return View(group);
        }

        [HttpPost]
		public async Task<IActionResult> Delete(int id)
        {
            // stergem toate intrarile din UserGroups
            var entries = db.UserGroups.Where(ug => ug.GroupId == id);
            foreach(var line in entries)
            {
                db.UserGroups.Remove(line);
            }

            // stergem toate postarile din grup
            var posts = db.Posts.Where(p => p.GroupId == id);
            foreach(var post in posts)
            {
                db.Posts.Remove(post);
            }

            var group = db.Groups.Where(g => g.Id == id).FirstOrDefault();
            db.Groups.Remove(group);
			await db.SaveChangesAsync();
			db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            Group grup = db.Groups.Find(id);
            var idUser = _userManager.GetUserId(User);
            if (grup.ModeratorId == idUser  || User.IsInRole("Admin"))
            {
                return View(grup);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile? Fotografie)
        {
            Group group = db.Groups.Find(id);
			var idUser = _userManager.GetUserId(User);
			if (!(group.ModeratorId == idUser || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                group.Nume = requestGroup.Nume;
                group.Descriere = requestGroup.Descriere;

                if (Fotografie != null && Fotografie.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                    var fileExtension = Path.GetExtension(Fotografie.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("PostImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                        return View(requestGroup);
                    }

                    var storagePath = Path.Combine(_env.WebRootPath, "images", Fotografie.FileName);
                    var databaseFileName = "/images/" + Fotografie.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await Fotografie.CopyToAsync(fileStream);
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
        //    [NonAction]
        //    public IEnumerable<SelectListItem> GetAllTags()
        //    {
        //        var selectList = new List<SelectListItem>();
        //        var tags = from tag in db.Tags
        //                   select tag;
        //        // iteram prin taguri
        //        foreach (var tag in tags)
        //        {
        //            selectList.Add(new SelectListItem
        //            {
        //                Value = tag.Id.ToString(),
        //                Text = tag.Denumire
        //            });
        //        }
        //        return selectList;
        //    }
    }
}