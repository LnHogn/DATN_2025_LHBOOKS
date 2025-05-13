using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LHBooksWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LHBooksWeb.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; }

        public bool isActive { get; set; }

        // Cập nhật phương thức GenerateUserIdentity để trả về ClaimsIdentity đúng
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tạo ClaimsIdentity với CookieAuthenticationDefaults.AuthenticationScheme thay vì DefaultAuthenticationTypes.ApplicationCookie
            var userIdentity = new ClaimsIdentity(await manager.GetClaimsAsync(this), CookieAuthenticationDefaults.AuthenticationScheme);

            // Thêm các claims tùy chỉnh nếu cần thiết
            // userIdentity.AddClaim(new Claim("FullName", this.FullName));

            return userIdentity;
        }

    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<ProductSubCategory> ProductSubCategories { get; set; }

        public DbSet<FlashSale> FlashSales { get; set; }
        public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<ProductReview> productReviews { get; set; }

        public DbSet<ContactModel> Contact { get; set; }
    }
}
