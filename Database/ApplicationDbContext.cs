using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using TourBookingSystem.Models;

namespace TourBookingSystem.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Order> Orders { get; set; }

        [Table("user_favorites")]
        public class Favorite
        {
            [Key]
            [Column("favorite_id")]
            public int Id { get; set; }
            [Column("user_id")]
            public int UserId { get; set; }
            [Column("tour_id")]
            public int TourId { get; set; }
            [Column("created_at")]
            public DateTime CreatedAt { get; set; }
        }

        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Any specific configurations can go here
            modelBuilder.Entity<ApplicationDbContext.Favorite>().ToTable("user_favorites");
        }
    }
}
