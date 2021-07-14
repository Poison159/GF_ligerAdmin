using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;

namespace WebOneApp.Models
{
    public class Helper
    {
        public static Token saveAppUserAndToken(AppUser appUser, ApplicationDbContext db)
        {
            var token = createToken(appUser.Email);
            db.AppUsers.Add(appUser);
            db.Tokens.Add(token);
            db.SaveChanges();
            return token;
        }

        internal static void removeDomain(List<BranchMeal> meals, ApplicationDbContext db)
        {
            foreach (var meal in meals)
                meal.imgPath = StripDomain(meal.imgPath);
            db.SaveChanges();
        }

        public static string StripDomain(string imgPath) {
            return imgPath.Split('/').Last();
        }

        public static void prepareBranch(ApplicationDbContext db, Branch branch) {

            var res = db.Resturants.ToList().First((x) => x.id == branch.restId);
            var restaurantMeals = db.Meals.Where(x => x.resturantId == res.id).ToList();
            foreach (var meal in restaurantMeals)
            {
                meal.restaurantName = db.Resturants.Find(meal.resturantId).name;
                meal.imgPath = Helper.appendDomain(meal.imgPath);
            }

            res.categories = Helper.SortMeals(restaurantMeals, res.meals);
            branch.resturant = res;
            branch.resturant.imgPath = Helper.appendDomain(branch.resturant.imgPath);
            branch.branchMeals = db.BranchMeals.Where(x => x.branchId == branch.id).ToList();
            foreach (var meal in branch.branchMeals)
            {
                meal.imgPath = Helper.appendDomain(meal.imgPath);
            }
            branch.reviews = db.Ratings.Where(x => x.branchId == branch.id).ToList();
            branch.reviews.Reverse();
            branch.rating = Helper.getAvgRating(branch.reviews);
            foreach (var rev in branch.reviews)
            {
                rev.username = db.AppUsers.First(x => x.Id == rev.userId).Name;
            }
        }

        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string appendDomain(string imgPath) {
            return System.Configuration.ConfigurationManager.AppSettings["prodUrl"] + "/Content/imgs/" + imgPath;
        }


        public static void createManager(string email, string password, string roleName, ApplicationDbContext context) {
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (!context.Users.Any(x => x.UserName == email))
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };
                userManager.Create(user, password);
                context.Roles.AddOrUpdate(x => x.Name, new IdentityRole { Name = roleName });
                context.SaveChanges();
                userManager.AddToRole(user.Id, roleName);
            }
        }

        internal static Dictionary<int, string> getResNames(List<Resturant> list)
        {
            var dict = new Dictionary<int, string>();
            foreach (var item in list){
                dict.Add(item.id, item.name);
            }
            return dict;
        }

        internal static Dictionary<string,string> getManNames(List<Branch> branchs, ApplicationDbContext db){
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var users = db.Users.ToList();
            var ret = new Dictionary<string, string>();
            foreach (var item in branchs){
                try
                {
                    ret.Add(item.userId, userManager.FindById(item.userId).Email);
                }
                catch (Exception){
                    continue;
                }
                
            }
            return ret;
        }

        internal static object checkOPeratingHours(List<OperatingHours> operatingHours, List<Branch> branches, ApplicationDbContext db)
        {
            var retOpHours = new List<OperatingHours>();
            var ids = getBranchIds(branches);
            foreach (var opHour in operatingHours)
            {
                if (!ids.Contains(opHour.branchId))
                {
                    db.OperatingHours.Remove(opHour);
                }
            }
            db.SaveChanges();
            return db.OperatingHours.ToList();
        }

        public static object CheckContact(string str) {
            int i = 0;
            int check;
            foreach (var item in str) {
                try {
                    int.TryParse(item.ToString(), out check);
                }
                catch (Exception) {
                    return new { Error = "numbers only allowed" };
                }
                i++;
            }
            if (i < 10 || i > 10) {
                return new { Error = "contact number must be 10 digits" };
            }
            return new { };
        }

        public static List<Branch> GetNearByLocations(string Currentlat, string Currentlng, int distance, List<Branch> branches)
        {
            try
            {
                var userLocationLat = Convert.ToDouble(Currentlat,CultureInfo.InvariantCulture);
                var userLocationLong = Convert.ToDouble(Currentlng, CultureInfo.InvariantCulture);

                foreach (var item in branches)
                {
                    var locationLat = Convert.ToDouble(item.lat, CultureInfo.InvariantCulture);
                    var locationLon = Convert.ToDouble(item.lon, CultureInfo.InvariantCulture);
                    var distanceToIndawo = distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K');
                    item.distance = Math.Round(distanceToIndawo);
                }
                List<Branch> nearLocations = getPlacesWithIn(branches, distance);
                return nearLocations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        public static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static double distanceToo(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }
        private static List<Branch> getPlacesWithIn(List<Branch> branches, int distance)
        {
            var finalList = new List<Branch>();
            foreach (var item in branches)
                if (item.distance <= distance)
                    finalList.Add(item);
            return finalList;
        }

        private static List<int> getBranchIds(List<Branch> branches)
        {
            var ret = new List<int>();
            foreach (var item in branches) { ret.Add(item.id); }
            return ret;
        }

        public static Dictionary<int,string> getAppUsers(List<AppUser> appUsers) {
            var dict = new Dictionary<int, string>();
            foreach (var item in appUsers){
                dict.Add(item.Id, item.Name);
            }
            return dict;
        }

        public static void downloadImage(string imageUrl, string downloadPath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(downloadPath), imageUrl);
            }
        }


        public static int getAvgRating(List<Rating> ratings) {
            var sum = 0;
            double avrg = 0;
            if (ratings.Count() == 0) {
                return 0;
            }
            foreach (var rating in ratings){
                sum += rating.rating;
            }
            avrg = sum / ratings.Count();
            return Convert.ToInt32(Math.Round(avrg));
        }

        internal static dynamic getIndivisualOperationhours(List<OperatingHours> operatingHours, List<Branch> branches)
        {
            var branche = Helper.getBranchNames(branches);
            var retHours = new Dictionary<string, List<OperatingHours>>();

            foreach (var indawo in branche)
            {
                retHours.Add(indawo.Value, operatingHours.Where(x => x.branchId == indawo.Key).ToList());
            }
            return retHours;
        }

        internal static dynamic getBranchNames(List<Branch> branches)
        {
            var strList = new Dictionary<int, string>();
            foreach (var item in branches.OrderBy(x => x.name))
            {
                strList.Add(item.id, item.name);
            }
            return strList;
        }

        internal static List<ApplicationUser> getMangers(List<ApplicationUser> list, ApplicationDbContext db)
        {
            return list.Where(x => x.Roles.Select(role => role.RoleId).Contains("a7dd8a8c-6e9b-4581-b961-e194225e707c")).ToList();
        }

        public static Token createToken(string email)
        {
            var tokenString = Guid.NewGuid().ToString();
            var grantDate = DateTime.Now;
            var endDate = grantDate.AddDays(90);
            return new Token(email, tokenString, grantDate, endDate);
        }

        public static List<string> getCategoryNames(List<Category> categories) {
            List<string> categoryList = new List<string>();
            foreach (var item in categories){
                categoryList.Add(item.name);
            }
            return categoryList;
        }

        internal static List<string> SortMeals(List<Meal> meals, Dictionary<string, List<Meal>> catAndMeal)
        {
            List<string> categories = getDistinctCategories(meals);
            foreach (var item in categories)
            {
                var listOfMeals = meals.Where(x => x.category == item).ToList();
                try
                {
                    catAndMeal.Add(item, listOfMeals);
                }
                catch (Exception)
                {

                    continue;
                }
                
            }
            return categories;
        }

        private static List<string> getDistinctCategories(List<Meal> list)
        {
            var strLst = new List<string>();
            foreach (var item in list){
                strLst.Add(item.category);
            }
            return strLst.Distinct().ToList();
        }
    }
}