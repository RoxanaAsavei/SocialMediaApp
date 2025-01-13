using Ganss.Xss;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SocialMediaApp.Controllers
{
	[Authorize(Roles = "User,Admin")]
	public class PostsController : Controller
	{
        private readonly ApplicationDbContext db;
		private readonly IWebHostEnvironment _env;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
	
		public PostsController(
        ApplicationDbContext context,
		IWebHostEnvironment env,
		UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
		SignInManager<ApplicationUser> signInManager
        )
        {
            db = context;
			_env = env;
            _userManager = userManager;
            _roleManager = roleManager;
			_signInManager = signInManager;
        }
        public IActionResult Index()
        {
			// daca nu e signed in, luam postarile persoanelor care au cont public
			var postari = db.Posts.Include("Comments")
                        .Include("Tag")
                        .Include("User")
                        .Where(p => p.User.Privacy == false && p.GroupId == null) // Exclude posts with a non-null GroupId
                        .OrderByDescending(a => a.Data);

            if (_signInManager.IsSignedIn(User)) // luam postarile persoanelor pe care le urmareste
                                                 // + postarile lui
            {
                // daca e admin luam toate postarile, indiferent 
                // daca contul e privat sau public
                if (User.IsInRole("Admin"))
                {
                    postari = db.Posts.Include("Comments")
                        .Include("Tag")
                        .Include("User")
                        .Where(p => p.GroupId == null)
                        .OrderByDescending(a => a.Data);
                }
                else
                {
                    // luam id-ul userului
                    var id = _userManager.GetUserId(User);
                    // luam lista oamenilor pe care ii urmareste
                    List<string> following = db.Follows
                                                     .Where(f => f.FollowerId == id)
                                                     .Select(f => f.FollowedId)
                                                     .ToList();
                    // luam postarile facute de oamenii pe care ii urmareste
                    postari = db.Posts.Include("Comments")
                           .Include("Tag")
                           .Include("User")
                           .Where(p => (following.Contains(p.UserId) || p.UserId == id) && p.GroupId == null) // Exclude posts with a non-null GroupId
                           .OrderByDescending(a => a.Data);
                }
            }

            var search = HttpContext.Request.Query["search"].ToString().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                List<int> postIds = db.Posts.Where(at => at.Continut.Contains(search)).Select(a => a.Id).ToList();
                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                    .Where(c => c.Continut.Contains(search))
                    .Select(c => (int)c.PostId).ToList();
                List<int> mergedIds = postIds.Union(articleIdsOfCommentsWithSearchString).ToList();
                postari = db.Posts.Where(postare => mergedIds.Contains(postare.Id) && postare.GroupId == null) // Exclude posts with a non-null GroupId
                                  .Include("Comments")
                                  .Include("Tag")
                                  .Include("User")
                                  .OrderByDescending(a => a.Data);
            }

            ViewBag.SearchString = search;

            int _perPage = 3;
            int totalItems = postari.Count();
            int currentPage = 1;
            if (int.TryParse(HttpContext.Request.Query["page"], out int page))
            {
                currentPage = page;
            }

            var offset = (currentPage - 1) * _perPage;
            var paginatedArticles = postari.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / _perPage);
            ViewBag.CurrentPage = currentPage;
            ViewBag.Posts = paginatedArticles;

            if (!string.IsNullOrEmpty(search))
            {
                ViewBag.PaginationBaseUrl = $"/Posts/Index?search={search}&page=";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Posts/Index?page=";
            }

            SetAccessRights();
            return View();
        }


        public IActionResult Show(int id)
		{
			Post post = db.Posts.Include("Tag")
						.Include("Comments").Include("User")
						.Include("Comments.User")
						.Where(post => post.Id == id)
						.First();
			ViewBag.Comms = db.Comments
										.Include("User")
										.Where(c => c.PostId == id)
										.OrderByDescending(c => c.Data);
			SetAccessRights();
			return View(post);
		}


        // adaugarea unui comentariu din formular
        [HttpPost]
		public IActionResult Show([FromForm] Comment comment)
		{
			// luam postarea la care s-a postat comentariul
			Post post = db.Posts
						.Where(post => post.Id == comment.PostId)
						.First();
			// incrementam numarul de comentarii
			post.NrComments++;
			comment.Data = DateTime.Now;
			comment.UserId = _userManager.GetUserId(User);
            if(ModelState.IsValid)
			{
				db.Comments.Add(comment);
				db.SaveChanges();
				return Redirect("/Posts/Show/" + comment.PostId);
			}
			else
			{

				Post post1 = db.Posts.Include("Tag")
						.Include("Comments").Include("User")
						.Include("Comments.User")
						.Where(post => post.Id == comment.PostId)
						.First();

				return View(post1);
			}
		}





		// edit cu get - afisarea formularului
		[HttpGet]
		public IActionResult Edit(int id)
		{
            Post post = db.Posts.Include("Tag")
						.Where(post => post.Id == id)
						.First();
			post.Tags = GetAllTags();
            if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
			{
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
				return RedirectToAction("Index");
            }
		}

		//edit cu post - salvarea datelor
		[HttpPost]
		public async Task<IActionResult> Edit(int id, Post requestPost, IFormFile? Image)
		{
			var sanitizer = new HtmlSanitizer();
			sanitizer.AllowedTags.Add("iframe");
			sanitizer.AllowedTags.Add("img");
			sanitizer.AllowedAttributes.Add("src");
			sanitizer.AllowedAttributes.Add("alt");
			Post post = await db.Posts.FindAsync(id);
            if (!(post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
			{
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }
            if (post == null)
			{
				return NotFound();
			}

			if (!ModelState.IsValid)
			{
				requestPost.Tags = GetAllTags();
				return View(requestPost);
			}

			try
			{
				post.Continut = sanitizer.Sanitize(requestPost.Continut);
				post.Data = DateTime.Now;
				post.TagId = requestPost.TagId;
				post.Locatie = requestPost.Locatie;
				//if(requestPost.Video != null)
				//{
				//		post.Video = GetYouTubeVideoId(post.Video);
				//}	

				if (Image != null)
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

				db.Entry(post).State = EntityState.Modified;

				TempData["message"] = "Postarea a fost editata";
				await db.SaveChangesAsync();
				return Redirect("/Posts/Show/" + post.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				requestPost.Tags = GetAllTags();
				return View(requestPost);
			}
		}

        [HttpGet]
        public IActionResult New(int? groupId = null)
        {
            var post = new Post
            {
                GroupId = groupId,
                Tags = GetAllTags()
            };
			ViewBag.Tags = GetAllTags();
            return View(post);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
		public async Task<IActionResult> New(Post post, IFormFile? Image)
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
			post.NrLikes = 0;
			post.UserId = _userManager.GetUserId(User);

			//if(post.Video != null)
			//{

			//	post.Video = GetYouTubeVideoId(post.Video);
			//}

			if (string.IsNullOrEmpty(post.UserId))
			{
				ModelState.AddModelError("UserId", "Unable to determine the user ID.");
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
                if (post.GroupId.HasValue)
                {
                    return RedirectToAction("Show", "Groups", new { id = post.GroupId.Value });
                }
                return RedirectToAction("Index", "Posts");
            }
			post.Tags = GetAllTags();
			return View(post);
		}


		//delete - cu post
		[HttpPost]
		public IActionResult Delete(int id)
		{
			var post = db.Posts.Find(id);
            if (!(post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
				return RedirectToAction("Index");
            }
            db.Posts.Remove(post);
			db.SaveChanges();
            if (post.GroupId.HasValue)
            {
                return RedirectToAction("Show", "Groups", new { id = post.GroupId.Value });
            }
            return RedirectToAction("Index");
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetAllTags()
		{
			var selectList = new List<SelectListItem>();
			var tags = from tag in db.Tags
					   select tag;
			// iteram prin taguri
			foreach(var tag in tags)
			{
				selectList.Add(new SelectListItem
				{
					Value = tag.Id.ToString(),
					Text = tag.Denumire
				});
			}
			return selectList;
		}


		private void SetAccessRights()
		{

			ViewBag.UserCurent = _userManager.GetUserId(User);
			ViewBag.EsteAdmin = User.IsInRole("Admin");
		}

		//private static string GetYouTubeVideoId(string url)
		//{
		//	var regex = new Regex(@"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})");
		//	var match = regex.Match(url);
		//	return match.Success ? match.Groups[1].Value : string.Empty;
		//}
	}
}
