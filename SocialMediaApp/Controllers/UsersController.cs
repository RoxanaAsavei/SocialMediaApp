using AngleSharp.Io;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
	public class UsersController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _env;
		private readonly RoleManager<IdentityRole> _roleManager;
		public UsersController(
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
        // afisam profilele tuturor utilizatorilor
		// la final trebuie autorizat doar pentru admin -> panoul de admin
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
		{
			var users = from user in db.ApplicationUsers
						select user;
			ViewBag.UsersList = users;
			return View();
		}

		// vizualizare profil -> ce vad eu din profilul tau
		public IActionResult Details(string? id)
		{
			
			if (string.IsNullOrEmpty(id))
			{
				return RedirectToAction("Login", "Account");
			}

			// afisez detaliile personale

			ApplicationUser user = db.Users.Find(id);
			if (user == null)
			{
				return NotFound();
			}
			// luam postarile utilizatorului
			var posts = db.Posts
								.Include(p => p.Comments)
								.Include(p => p.Tag)
								.Where(post => post.UserId == id && post.GroupId == null)
								.ToList();

			int noFollowers = db.Follows.Where(f => f.FollowedId == id && f.Accepted == true).Count();
			int noFollowing = db.Follows.Where(f => f.FollowerId == id && f.Accepted == true).Count();
			int noPosts = db.Posts.Where(p => p.UserId == id && p.GroupId == null).Count();

			ViewBag.UserCurent = _userManager.GetUserId(User);
			ViewBag.EsteAdmin = User.IsInRole("Admin");
			ViewBag.noFollowers = noFollowers;
			ViewBag.noFollowing = noFollowing;
			ViewBag.noPosts = noPosts;
			ViewBag.Posts = posts;


			// vad daca il urmaresc / i-am dat follow
			bool? accepted = db.Follows.
				Where(f => f.FollowerId == _userManager.GetUserId(User) && f.FollowedId == id)
				.Select(f => f.Accepted).FirstOrDefault();
			if (accepted != null) 
			{ 
				ViewBag.Accepted = accepted;
			}

			ViewBag.IsMe = _userManager.GetUserId(User) == id;
			ViewBag.SeePosts = SetViewRights(id); // ca sa stim cat afisam din informatii
			return View(user);	
		}

		private bool SetViewRights(string id)
		{
			ApplicationUser user = db.Users.Find(id);
			// luam userul curent
			
			// daca contul e public, daca e contul meu sau daca sunt admin, afisez tot
			if (user.Privacy == false || user.Id == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				return true;
			}
			else
			{
				return false;
			}
		
		}

		// new - cu get, formularul de inregistrare
		public IActionResult New()
		{
			return View();
		}
		// new - cu post, inregistrarea efectiva
		[HttpPost]
		public IActionResult New([FromForm] ApplicationUser user)
		{
			if(ModelState.IsValid)
			{
				db.ApplicationUsers.Add(user);
				db.SaveChanges();
				return Redirect("/Users/Index");
			}
			else
			{
				return Redirect("/Users/New");
			}
		}

		// edit - cu get, formularul de editare
		public IActionResult Edit(string id)
		{
			ApplicationUser? user = db.ApplicationUsers.Find(id);
			if (user == null)
			{
				return NotFound();
			}

			// vedem daca are drepturi de edit
			if (id == _userManager.GetUserId(User))
			{
				return View(user);
			}

			else
			{
				TempData["message"] = "Nu puteti edita un profil care nu va apartine";
				return Redirect("/Users/Details/" + id);
			}
		}

		// edit - cu post, editarea efectiva + salvarea in baza de date

		[HttpPost]
		public async Task<IActionResult> Edit(string id, ApplicationUser requestUser, IFormFile? Image)
		{

			ApplicationUser user = await db.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			if (!ModelState.IsValid)
			{	if(Image == null)
				{
					requestUser.Image = user.Image;
				}
				return View(requestUser);
			}

			try
			{	
				user.FirstName = requestUser.FirstName;
				user.LastName = requestUser.LastName;
				user.Description = requestUser.Description;
				// daca s-a schimbat vizibilitatwa contului, accept toate cererile de urmarire by default
				if(user.Privacy == true && requestUser.Privacy == false)
				{
					//iau toate cererile care il au pe followedId = id
					var follows = db.Follows.Where(f => f.FollowedId == id && f.Accepted == false).ToList();
					foreach (var follow in follows)
					{
						follow.Accepted = true;
						follow.Date = DateTime.Now;
					}
					db.SaveChanges();
				}
				user.Privacy = requestUser.Privacy;

				if (Image != null)
				{
					// Verificăm extensia
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
					var fileExtension = Path.GetExtension(Image.FileName).ToLower();
					if (!allowedExtensions.Contains(fileExtension))
					{
						ModelState.AddModelError("ProfileImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
						return View(requestUser);
					}

					// Cale stocare
					var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
					var databaseFileName = "/images/" + Image.FileName;

					// Salvare fișier
					using (var fileStream = new FileStream(storagePath, FileMode.Create))
					{
						await Image.CopyToAsync(fileStream);
					}
					ModelState.Remove(nameof(user.Image));
					user.Image = databaseFileName;
				}

				db.Entry(user).State = EntityState.Modified;

				TempData["message"] = "Profilul a fost editat";
				await db.SaveChangesAsync();
				return Redirect("/Users/Details/" + user.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				return View(requestUser);
			}
		}
		
		public IActionResult Search()
		{
			// search pe useri
			// search pe useri
			var search = HttpContext.Request.Query["search"].ToString().Trim();
			if (!string.IsNullOrEmpty(search))
			{
				// luam userii care contin in prenume stringul cautat
				List<string> userIdFirstName = db.ApplicationUsers.Where(usr => usr.FirstName.Contains(search)).Select(u => u.Id).ToList();
				// luam userii care contin in nume stringul cautat
				List<string> userIdLastName = db.ApplicationUsers.Where(usr => usr.LastName.Contains(search)).Select(u => u.Id).ToList();
				// luam userii care contin in username stringul cautat
				List<string> userIdUserName = db.ApplicationUsers.Where(usr => usr.UserName.Contains(search)).Select(u => u.Id).ToList();
				List<string> mergedIds = userIdFirstName.Union(userIdLastName).Union(userIdUserName).ToList();
				ViewBag.Users = db.Users.Where(user => mergedIds.Contains(user.Id)).ToList();
				ViewBag.UsersCt = db.Users.Where(user => mergedIds.Contains(user.Id)).ToList().Count();
			}
			ViewBag.SearchUser = search;
			return View();
		}

		public IActionResult Settings(string id)
		{
			// luam userul curent si il trimitem ca model
			ApplicationUser user = db.Users.Find(id);
			if(user == null)
			{
				return NotFound();
			} 
				
			return View(user); 
		}

		[HttpPost]
		public IActionResult ChangePassword()
		{
			var user = db.Users.Find(_userManager.GetUserId(User));
			if (user == null)
			{
				return NotFound();
			}
			return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
		}
	}
}
