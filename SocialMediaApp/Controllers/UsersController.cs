using AngleSharp.Io;
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
		private readonly RoleManager<IdentityRole> _roleManager;
		public UsersController(
		ApplicationDbContext context,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager
		)
		{
			db = context;
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

			return View(user);
		}

		// edit - cu post, editarea efectiva + salvarea in baza de date

		[HttpPost]
		public IActionResult Edit(string id, [FromForm] ApplicationUser user)
		{
			if (ModelState.IsValid)
			{
				try
				{
					ApplicationUser? userToEdit = db.ApplicationUsers.Find(id);
					if (userToEdit != null && TryUpdateModelAsync(userToEdit).Result)
					{
						db.SaveChanges();
					}
					return Redirect("/Users/Details/" + id);
				}
				catch (Exception)
				{
					return Redirect("/Users/Edit/" + id);
				}
			}
			return Redirect("/Users/Edit/" + id);
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
