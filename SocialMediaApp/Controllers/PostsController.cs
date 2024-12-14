﻿using Ganss.Xss;
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
	public class PostsController : Controller
	{
        private readonly ApplicationDbContext db;
		private readonly IWebHostEnvironment _env;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(
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
            var postari = db.Posts.Include("Comments")
                                    .Include("Tag")
                                    .Include("User")
                                    .OrderByDescending(a => a.Data);

            var search = HttpContext.Request.Query["search"].ToString().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                List<int> postIds = db.Posts.Where(at => at.Continut.Contains(search)).Select(a => a.Id).ToList();
                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                    .Where(c => c.Continut.Contains(search))
                    .Select(c => (int)c.PostId).ToList();
                List<int> mergedIds = postIds.Union(articleIdsOfCommentsWithSearchString).ToList();
                postari = db.Posts.Where(postare => mergedIds.Contains(postare.Id))
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

            return View();
        }


        public IActionResult Show(int id)
        {
            Post post = db.Posts.Include("Tag")
                        .Include("Comments").Include("User")
                        .Include("Comments.User")
                        .Where(post => post.Id == id)
                        .First();
            return View(post);
        }

		// adaugarea unui comentariu din formular
		[HttpPost]
		public IActionResult Show([FromForm] Comment comment)
		{
			comment.Data = DateTime.Now;
			comment.UserId = _userManager.GetUserId(User);
			try
			{
				db.Comments.Add(comment);
				db.SaveChanges();
				return Redirect("/Posts/Show/" + comment.PostId);
			}
			catch(Exception)
			{

				Post post = db.Posts.Include("Tag")
						.Include("Comments").Include("User")
						.Include("Comments.User")
						.Where(post => post.Id == comment.PostId)
						.First();

				return View(post);
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
			return View(post);
		}

		//edit cu post - salvarea datelor
	   [HttpPost]
		public IActionResult Edit(int id, Post requestPost)
		{
            var sanitizer = new HtmlSanitizer();
            Post post = db.Posts.Find(id);
			try
			{
                requestPost.Continut = sanitizer.Sanitize(requestPost.Continut);
                post.Continut = requestPost.Continut;
                post.Data = DateTime.Now;
				post.TagId = requestPost.TagId;
				post.Locatie = requestPost.Locatie;
				TempData["message"] = "Articolul a fost modificat";
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			catch (Exception)
			{
				requestPost.Tags = GetAllTags();
				return View(requestPost);
			}
		}


		// new cu get - afisarea formularului
		[HttpGet]
		public IActionResult New()
		{
			Post post = new Post();
			post.Tags = GetAllTags();
			return View(post);
		}
        //[HttpPost]
        //public IActionResult New(Post post)
        //{
        //	post.Data = DateTime.Now;
        //	post.UserId = _userManager.GetUserId(User);
        //	try
        //	{
        //		db.Posts.Add(post);
        //		db.SaveChanges();
        //		TempData["message"] = "Articolul a fost adaugat";
        //		return RedirectToAction("Index");
        //	}
        //	catch (Exception)
        //	{
        //		post.Tags = GetAllTags();
        //		return View(post);
        //	}
        //}

        [HttpPost]
		public async Task<IActionResult> New(Post post, IFormFile Image)
		{
			if (User.Identity == null || !User.Identity.IsAuthenticated)
			{
				ModelState.AddModelError("UserId", "User must be logged in to create a post.");
				post.Tags = GetAllTags();
				return View(post);
			}
            var sanitizer = new HtmlSanitizer();
            post.Data = DateTime.Now;
			post.UserId = _userManager.GetUserId(User);

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
                // Adăugare articol
                db.Posts.Add(post);
				await db.SaveChangesAsync();
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
			db.Posts.Remove(post);
			db.SaveChanges();
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
	}
}
