using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebOneApp.Models;

namespace WebOneApp.Controllers
{
    [HandleError]
    public class ResturantsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Resturants
        [HandleError]
        public ActionResult Index()
        {
            var restaurants = db.Resturants.ToList();
            //Helper.removeDomain(db.BranchMeals.ToList(), db);
            foreach (var res in restaurants)
                res.imgPath = Helper.appendDomain(res.imgPath);
            return View(restaurants);
        }

        // GET: Resturants/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resturant resturant = db.Resturants.Find(id);
            resturant.imgPath = Helper.appendDomain(resturant.imgPath);
            var currMeals = db.Meals.Where(x => x.resturantId == resturant.id).ToList();
            foreach (var meal in currMeals)
                meal.imgPath = Helper.appendDomain(meal.imgPath);
            
            resturant.categories = Helper.SortMeals(currMeals, resturant.meals);
            if (resturant == null)
            {
                return HttpNotFound();
            }
            
            return View(resturant);
        }

        public ActionResult AddMeal(int? resId) {
            var meal = new Meal() {resturantId = Convert.ToInt32(resId) };
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            ViewBag.resturant = db.Resturants.First(x => x.id == meal.resturantId).name;
            return View(meal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMeal([Bind(Include ="id,resturantId,category,name,price,description,startDate,endDate,imgPath")] Meal meal, int? resId)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            ViewBag.resturant = db.Resturants.First(x => x.id == resId).name;
            if (ModelState.IsValid)
            {
                if (db.Branches.Where(x => x.name.Replace(" ", "").Trim().ToLower() ==
                        meal.name.Replace(" ", "").Trim().ToLower() && x.restId == meal.resturantId).Count() == 0)
                {
                    string targetPath = Server.MapPath("~");
                    var randString = Helper.RandomString(7);
                    var path = targetPath + @"Content\imgs\" + randString + ".png";
                    try
                    {
                        Helper.downloadImage(path, meal.imgPath);
                        meal.imgPath = randString + ".png";
                        meal.resturantId = Convert.ToInt32(resId);
                        db.Meals.Add(meal);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return View(meal);
                    }
                    
                    return RedirectToAction("Details", "Resturants", new { id = meal.resturantId });
                }
                else {
                    ViewBag.error = "meal already exists";
                }
                
            }
            return View();
        }
        public ActionResult EditMeal(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Meal meal = db.Meals.Find(id);
            if (meal == null)
            {
                return HttpNotFound();
            }

            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()), meal.category);
            

            return View(meal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMeal([Bind(Include = "id,name,price,description,category,startDate,endDate,imgPath")] Meal meal,int restId)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, meal.imgPath);
                    meal.imgPath = randString + ".png";
                    meal.resturantId = restId;
                    db.Entry(meal).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(meal);
                }
               
                return RedirectToAction("Details", "Resturants", new { id = meal.resturantId });
            }
            return View(meal);
        }

        // GET: Resturants/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Resturants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,imgPath")] Resturant resturant)
        {
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, resturant.imgPath);
                    resturant.imgPath = randString + ".png";
                    db.Resturants.Add(resturant);
                    db.SaveChanges();
                }
                catch (Exception){
                    return View(resturant);
                }
                
                return RedirectToAction("Index");
            }

            return View(resturant);
        }

        // GET: Resturants/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resturant resturant = db.Resturants.Find(id);
            if (resturant == null)
            {
                return HttpNotFound();
            }
            return View(resturant);
        }

        // POST: Resturants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,imgPath")] Resturant resturant)
        {
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, resturant.imgPath);
                    resturant.imgPath = randString + ".png";
                    db.Entry(resturant).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    return View(resturant);
                }
                
            }
            return View(resturant);
        }

        // GET: Resturants/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resturant resturant = db.Resturants.Find(id);
            if (resturant == null)
            {
                return HttpNotFound();
            }
            return View(resturant);
        }

        // POST: Resturants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Resturant resturant = db.Resturants.Find(id);
            var branches = db.Branches.ToList().Where(x => x.restId == resturant.id);
            foreach (var item in branches){
                var ratings = db.Ratings.Where(x => x.branchId == item.id);
                var branchMeals = db.BranchMeals.Where(x => x.branchId == item.id);
                foreach (var rating in ratings){
                    db.Ratings.Remove(rating); 
                }
                foreach (var special in branchMeals){
                    db.BranchMeals.Remove(special);
                }
                db.Branches.Remove(item);
            }
            db.Resturants.Remove(resturant);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
