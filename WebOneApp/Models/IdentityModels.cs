using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebOneApp.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("OneMenu", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<WebOneApp.Models.Resturant> Resturants { get; set; }

        public System.Data.Entity.DbSet<WebOneApp.Models.Meal> Meals { get; set; }

        public System.Data.Entity.DbSet<WebOneApp.Models.Category> Categories { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        public System.Data.Entity.DbSet<WebOneApp.Models.Branch> Branches { get; set; }

        public System.Data.Entity.DbSet<WebOneApp.Models.OperatingHours> OperatingHours { get; set; }

        public System.Data.Entity.DbSet<WebOneApp.Models.BranchMeal> BranchMeals { get; set; }
        public System.Data.Entity.DbSet<WebOneApp.Models.Rating> Ratings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}