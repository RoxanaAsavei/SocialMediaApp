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
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
		{
			var users = from user in db.ApplicationUsers
						select user;
			ViewBag.Users = users;
			return View();
		}

		// show - detalii si feed pentru un utilizator
		// ca detalii - afisez datele personale si grupurile din care face parte
		public IActionResult Details(string id)
		{
			ApplicationUser user = db.ApplicationUsers.Include("Groups").Include("UserGroups")
						.Where(user => user.Id == id)
						.First();
			return View(user);	
		}

		// feed - afisez postarile utilizatorului
		public IActionResult Feed(string id)
		{ 
			Post posts = db.Posts.Include("User").Include("Comments").Include("Tags")
						.Where(post => post.UserId == id)
						.First();
			return View(posts);
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
					return Redirect("/Users/Index");
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
