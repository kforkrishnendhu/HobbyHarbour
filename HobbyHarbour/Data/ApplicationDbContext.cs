using HobbyHarbour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HobbyHarbour.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Cart> Cart { get; set; }

        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<MerchantOrder> MerchantOrders { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<WalletHistory> WalletHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                // Other configurations...

                // Configure the 'TotalAmount' property with precision and scale
                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(18, 2)"); // Adjust precision and scale as needed
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                // Other configurations...

                // Configure the 'TotalPrice' property with precision and scale
                entity.Property(e => e.TotalPrice)
                      .HasColumnType("decimal(18, 2)"); // Adjust precision and scale as needed
            });

            base.OnModelCreating(modelBuilder);

            //here we can write code for seed any database entity or table. for example look at below
            //modelBuilder.Entity<Category>().HasData(
            //	new Category { CategoryID = 1, CategoryName = "Soft Toys", Description = "Soft Toys for kids" },

            //             new Category { CategoryID = 2, CategoryName = "Hobby Kit", Description = "Hobby kit for kids" },

            //             new Category { CategoryID = 3, CategoryName = "Board Games", Description = "Board games for kids" }

            //             );
        }
    }
}


