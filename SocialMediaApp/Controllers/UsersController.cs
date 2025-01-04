using AngleSharp.Io;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

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
		public IActionResult Details(string id)
		{   // afisez detaliile personale
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

			// daca nu are vizibilitatea profilului setata, o punem pe public
			if (user.Privacy == null)
			{
				user.Privacy = false;
				db.SaveChanges();
			}

			// daca nu are imaginea setata, o punem pe default
			if (user.Image == null)
			{
				user.Image = "/images/default_image.jpg";
				db.SaveChanges();
			}

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
			{
				return View(requestUser);
			}

			try
			{
				user.FirstName = requestUser.FirstName;
				user.LastName = requestUser.LastName;
				user.Description = requestUser.Description;
				user.UserName = requestUser.UserName;
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

				TempData["message"] = "Profilul a fost editata";
				await db.SaveChangesAsync();
				return Redirect("/Users/Details/" + user.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				return View(requestUser);
			}
		}
		public IActionResult Show(string id) // by default, ma duce pe pagina de feed a utilizatorului
		{
			// pentru profilul unui utilizator o sa am 2 pagini: una de tip feed, 
			// unde o sa afisez postarile utilizatorului si alta de tip profile,
			// unde o sa afisez informatii despre utilizator
			ApplicationUser user = db.ApplicationUsers.Include("Posts")
						.Include("Comments").Include("Tags").Include("Groups").Include("UserGroups")
						.Where(user => user.Id == id)
						.First();
			return View();
		}
	}
}
