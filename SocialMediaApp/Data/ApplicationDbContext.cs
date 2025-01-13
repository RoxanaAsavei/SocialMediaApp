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
        public DbSet<Follow> Follows { get; set; }

		public DbSet<Like> Likes { get; set; }
		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Join> Joins { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

			// definim structura pentru like
			modelBuilder.Entity<Like>()
				.HasKey(l => new { l.PostId, l.UserId });

            modelBuilder.Entity<Like>()
				.HasOne(l => l.User)
				.WithMany(u => u.Likes)
				.HasForeignKey(l => l.UserId)
				.OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
				.WithMany(p => p.Likes)
				.HasForeignKey(l => l.PostId)
				.OnDelete(DeleteBehavior.Cascade);

			// definire primary key compus
			modelBuilder.Entity<Follow>()
			.HasKey(f => new { f.FollowerId, f.FollowedId });

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(u => u.Followers)
				.WithOne(f => f.Followed)
				.HasForeignKey(f => f.FollowedId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(u => u.Following)
				.WithOne(f => f.Follower)
				.HasForeignKey(f => f.FollowerId)
				.OnDelete(DeleteBehavior.Restrict);

			// definire primary key compus
			modelBuilder.Entity<UserGroup>()
		   .HasKey(ug => new { ug.UserId, ug.GroupId });

			modelBuilder.Entity<UserGroup>()
				.HasOne(ug => ug.User)
				.WithMany(u => u.UserGroups)
				.HasForeignKey(ug => ug.UserId);

			modelBuilder.Entity<UserGroup>()
				.HasOne(ug => ug.Group)
				.WithMany(g => g.UserGroups)
				.HasForeignKey(ug => ug.GroupId);


			// cand sterg o postare, vreau sa se stearga si toate comentariile
            // de la postarea respectiva
			modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // cand sterg tagul, nu vreau sa se stearga si postarea
            // pot sa am si postari fara tag
            // TagId devine null
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.TagId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Post>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}