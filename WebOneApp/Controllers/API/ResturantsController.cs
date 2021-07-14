using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using WebOneApp.Models;

namespace WebOneApp.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ResturantsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Resturants
        public IQueryable<Resturant> GetResturants()
        {
            var res = db.Resturants;
            foreach (var item in res){
                item.imgPath = Helper.appendDomain(item.imgPath);
            }
            return res;
        }

        // GET: api/Resturants/5
        [ResponseType(typeof(Resturant))]
        public object GetResturant(string code)
        {
            try
            {
                Resturant resturant = db.Resturants.ToList().First(x => x.guid == code);
                resturant.categories = Helper.SortMeals(db.Meals.Where(x => x.resturantId == resturant.id).ToList(), resturant.meals);
                resturant.imgPath = Helper.appendDomain(resturant.imgPath);
                if (resturant == null){
                    return NotFound();
                }

                return Ok(resturant);
            }catch(Exception) {
                return new { Errors = "Could not find resturant" };
            }
            
        }

        // PUT: api/Resturants/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutResturant(int id, Resturant resturant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != resturant.id)
            {
                return BadRequest();
            }

            db.Entry(resturant).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResturantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Resturants
        [ResponseType(typeof(Resturant))]
        public IHttpActionResult PostResturant(Resturant resturant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Resturants.Add(resturant);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = resturant.id }, resturant);
        }

        [Route("api/Rating")]
        [HttpGet]
        public object AddRating(int userId, int userRating,int branchId, string comment)
        {
            var user = db.AppUsers.Find(userId);
            if (String.IsNullOrEmpty(user.MobileNumber)){
                user.MobileNumber = branchId.ToString();
            }
            else {
                user.MobileNumber = user.MobileNumber + "," + branchId;
            }
            var rating = new Rating() {userId = userId, rating = userRating, branchId = branchId, comment = comment };
            if (db.Ratings.ToList().Where(x => x.branchId == branchId && x.userId == userId).ToList().Count() == 0){
                db.Ratings.Add(rating);
                db.SaveChanges();
                rating.username = db.AppUsers.Find(rating.userId).Name;
                return rating;
            }
            else {
                return new { Errors="Could not add rating"};
            }
        }

        [Route("api/Categories")]
        [HttpGet]
        public Dictionary<string, List<Meal>> Categories()
        {
            var categories = new List<String>() { "breakfast", "beef", "salads", "sandwiches", "dessert" };
            var ret = new Dictionary<string, List<Meal>>();
            var meals = db.Meals.ToList();
            foreach (var meal in meals){
                meal.imgPath = Helper.appendDomain(meal.imgPath);
            }
            foreach (var category in categories)
            {
                foreach (var meal in meals)
                {
                    if (meal.category.ToLower() == category) {
                        if (!ret.ContainsKey(category))
                        {
                            var list = new List<Meal>();
                            ret.Add(category, list);
                        }
                        else {
                            ret[category].Add(meal);
                        }
                    }
                }
            }
            return ret;
        }

        [Route("api/UserRatings")]
        [HttpGet]
        public List<int> AddRating(int userId)
        {
            var ratings = db.Ratings.ToList().Where(x => x.userId == userId).ToList();
            var retList = new List<int>();
            foreach (var item in ratings)
            {
                retList.Add(item.branchId);
            }
            return retList;
        }

        [Route("api/UserReservations")]
        [HttpGet]
        public List<Reservation> UserReservations(string email, int branchId){
           var reservations = db.Reservations.ToList().Where(x => x.email == email && x.branchId == branchId && x.status != "Cancelled").ToList();
            var retList = new List<Reservation>();
            foreach (var item in reservations){
                retList.Add(item);
            }
            return retList;
        }

        [Route("api/cancelReservation")]
        [HttpGet]
        public object cancelReservation(int resId)
        {
            try
            {
                var reservation = db.Reservations.First(x => x.id == resId);
                reservation.status = "Cancelled";
                db.SaveChanges();
                return reservation;
            }
            catch (Exception ex)
            {
                return new {Error = ex.Message};
            }
            
        }

        // DELETE: api/Resturants/5
        [ResponseType(typeof(Resturant))]
        public IHttpActionResult DeleteResturant(int id)
        {
            Resturant resturant = db.Resturants.Find(id);
            if (resturant == null)
            {
                return NotFound();
            }

            db.Resturants.Remove(resturant);
            db.SaveChanges();

            return Ok(resturant);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ResturantExists(int id)
        {
            return db.Resturants.Count(e => e.id == id) > 0;
        }
    }
}