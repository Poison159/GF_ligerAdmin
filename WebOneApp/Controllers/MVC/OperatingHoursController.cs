using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebOneApp.Models;

namespace WebOneApp.Controllers.MVC
{
    public class OperatingHoursController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public List<string> daysOfweek = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        // GET: OperatingHours
        public ActionResult Index()
        {
            var branches = db.Branches.ToList();
            var operatingHours = db.OperatingHours.ToList();
            ViewBag.indawoNames = Helper.getBranchNames(branches);
            ViewBag.sortedHours = Helper.getIndivisualOperationhours(operatingHours, branches);
            return View(Helper.checkOPeratingHours(operatingHours, branches, db));
        }

        // GET: OperatingHours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OperatingHours operatingHours = db.OperatingHours.Find(id);
            if (operatingHours == null)
            {
                return HttpNotFound();
            }
            return View(operatingHours);
        }

        // GET: OperatingHours/Create
        public ActionResult Create()
        {
            ViewBag.branchId = new SelectList(db.Branches, "id", "name");
            ViewBag.day = new SelectList(daysOfweek);
            return View();
        }

        // POST: OperatingHours/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,branchId,day,openingHour,closingHour")] OperatingHours operatingHours)
        {
            ViewBag.indawoId = new SelectList(db.Branches, "id", "name", operatingHours.branchId);
            ViewBag.day = new SelectList(daysOfweek, operatingHours.day);
            if (ModelState.IsValid)
            {
                db.OperatingHours.Add(operatingHours);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(operatingHours);
        }

        // GET: OperatingHours/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OperatingHours operatingHours = db.OperatingHours.Find(id);
            if (operatingHours == null)
            {
                return HttpNotFound();
            }
            return View(operatingHours);
        }

        // POST: OperatingHours/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,branchId,day,openingHour,closingHour")] OperatingHours operatingHours)
        {
            if (ModelState.IsValid)
            {
                db.Entry(operatingHours).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(operatingHours);
        }

        // GET: OperatingHours/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OperatingHours operatingHours = db.OperatingHours.Find(id);
            if (operatingHours == null)
            {
                return HttpNotFound();
            }
            return View(operatingHours);
        }

        // POST: OperatingHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OperatingHours operatingHours = db.OperatingHours.Find(id);
            db.OperatingHours.Remove(operatingHours);
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
