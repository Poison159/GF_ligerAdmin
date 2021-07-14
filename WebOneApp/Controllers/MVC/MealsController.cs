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
    public class MealsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Meals
        public ActionResult Index()
        {
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            var meals = db.Meals.ToList();
            
            return View(meals);
        }

        // GET: Meals/Details/5
        public ActionResult Details(int? id)
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
            return View(meal);
        }

        // GET: Meals/Create
        public ActionResult Create()
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            return View();
        }

        // POST: Meals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,resturantId,name,price,description,category,startDate,endDate,imgPath")] Meal meal)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            if (ModelState.IsValid)
            {
                if (db.Branches.Where(x => x.name.Replace(" ", "").Trim().ToLower() ==
                        meal.name.Replace(" ", "").Trim().ToLower() && x.restId == meal.resturantId).Count() == 0) {
                    db.Meals.Add(meal);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else{
                    ViewBag.error = "meal already exists";
                }

            }

            return View(meal);
        }

        // GET: Meals/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name", meal.resturantId);

            return View(meal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,resturantId,name,price,description,category,startDate,endDate,imgPath")] Meal meal)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.resturantId = new SelectList(db.Resturants, "id", "name");
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, meal.imgPath);
                    meal.imgPath = randString + ".png";
                    db.Entry(meal).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(meal);
                }
                
                return RedirectToAction("Details","Resturants", new { id = meal.resturantId});
            }
            return View(meal);
        }

        // GET: Meals/Delete/5
        public ActionResult Delete(int? id)
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
            return View(meal);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Meal meal = db.Meals.Find(id);
            db.Meals.Remove(meal);
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
