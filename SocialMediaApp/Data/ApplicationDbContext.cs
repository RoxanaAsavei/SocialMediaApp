using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models;

namespace SocialMediaApp.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Comment> Comments { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Group> Groups { get; set; }
		public DbSet<UserGroup> UserGroups { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure cascade delete for Comment and Post
			modelBuilder.Entity<Comment>()
				.HasOne(c => c.Post)
				.WithMany(p => p.Comments)
				.HasForeignKey(c => c.PostId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure foreign key for Post and User with NO ACTION
			modelBuilder.Entity<Post>()
				.HasOne(p => p.User)
				.WithMany(u => u.Posts)
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			// Configure foreign key for Tag and User with NO ACTION
			modelBuilder.Entity<Tag>()
				.HasOne(t => t.User)
				.WithMany(u => u.Tags)
				.HasForeignKey(t => t.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			// Configure foreign key for Comment and User with NO ACTION
			modelBuilder.Entity<Comment>()
				.HasOne(c => c.User)
				.WithMany(u => u.Comments)
				.HasForeignKey(c => c.UserId)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}

