﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

        public DbSet<GroupModerator> GroupModerators { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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


			// Configure cascade delete for Comment and Post
			modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade delete for Tag and Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.TagId)
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

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}