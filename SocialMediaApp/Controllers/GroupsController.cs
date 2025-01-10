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
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            var grupuri = db.Groups
                            .Include(g => g.UserGroups)
                            .Include(g => g.Moderators)
                            .ToList();
            var userGroups = db.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToList();
            var moderatedGroups = db.GroupModerators.Where(gm => gm.UserId == userId).Select(gm => gm.GroupId).ToList();
            var joinRequests = db.Joins.Where(j => j.UserId == userId && j.Accepted == false).ToList();
            ViewBag.JoinRequests = joinRequests;
            ViewBag.Groups = grupuri;
            ViewBag.UserGroups = userGroups;
            ViewBag.ModeratedGroups = moderatedGroups;
            ViewBag.UserRoles = userRoles;
            return View();
        }

        [Authorize(Roles = "User,Moderator,Admin")]
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
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> JoinRequests(int groupId)
        {
            var joinRequests = await db.Joins
                .Include(j => j.User)
                .Where(j => j.GroupId == groupId && j.Accepted == false)
                .ToListAsync();

            ViewBag.GroupId = groupId;
            return View(joinRequests);
        }

        [Authorize(Roles = "Moderator,Admin")]
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

        [Authorize(Roles = "Moderator,Admin")]
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

        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);
            var isModerator = await db.GroupModerators.AnyAsync(gm => gm.UserId == userId && gm.GroupId == id);
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

        [Authorize(Roles = "Moderator,Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveUser(int groupId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isModerator = await db.GroupModerators.AnyAsync(gm => gm.UserId == currentUserId && gm.GroupId == groupId);
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

        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> UsersInGroup(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            var group = await db.Groups
                                .Include(g => g.UserGroups)
                                .ThenInclude(ug => ug.User)
                                .FirstOrDefaultAsync(g => g.Id == id);
            var moderatedGroups = db.GroupModerators.Where(gm => gm.UserId == userId).Select(gm => gm.GroupId).ToList();
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

        [Authorize(Roles = "Moderator,Admin")]
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
                db.Groups.Add(group);
                await db.SaveChangesAsync();
                var userId = _userManager.GetUserId(User);
                var groupModerator = new GroupModerator
                {
                    UserId = userId,
                    GroupId = group.Id
                };
                db.GroupModerators.Add(groupModerator);
                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = group.Id
                };
                db.UserGroups.Add(userGroup);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Groups");
            }
            return View(group);
        }
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Show(int id)
        {
            var group = await db.Groups
                .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                .Include(g => g.Posts)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var creator = await db.GroupModerators
                .Where(gm => gm.GroupId == id)
                .OrderBy(gm => gm.Id)
                .FirstOrDefaultAsync();

            var isCreator = creator != null && creator.UserId == userId;
            ViewBag.CanDeleteGroup = isAdmin || isCreator;

            return View(group);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var group = db.Groups.Include(g => g.Moderators).FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!(User.IsInRole("Moderator") || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }

            db.GroupModerators.RemoveRange(group.Moderators);
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

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile? Image)
        {
            Group group = db.Groups.Find(id);
            if (!(User.IsInRole("Moderator") || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
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