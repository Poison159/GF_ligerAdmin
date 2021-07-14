using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebOneApp.Models;

namespace WebOneApp.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    [HandleError]
    public class BranchesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Branches
        public ActionResult Index()
        {
            List<Branch> branchs = branchs = db.Branches.ToList();
            if (User.IsInRole("Manager"))
            {
                branchs = branchs.Where(x => x.userId == User.Identity.GetUserId()).ToList();
            }
            foreach (var branch in branchs)
            {
                branch.rating = Helper.getAvgRating(db.Ratings.Where(x => x.branchId == branch.id).ToList());
                branch.resturant = db.Resturants.Find(branch.restId);
            }
            ViewBag.names = Helper.getManNames(branchs, db);
            ViewBag.res = Helper.getResNames(db.Resturants.ToList());
            return View(branchs.OrderByDescending(x => x.rating));
        }

        // GET: Branches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branches.Find(id);
            var res = db.Resturants.ToList().First((x) => x.id == branch.restId);
            res.categories = Helper.SortMeals(db.Meals.Where(x => x.resturantId == res.id).ToList(), res.meals);
            branch.resturant = res;
            branch.branchMeals = db.BranchMeals.Where(x => x.branchId == branch.id).ToList();
            ViewBag.ratings = db.Ratings.Where(x => x.branchId == branch.id).ToList();
            ViewBag.appUsers = Helper.getAppUsers(db.AppUsers.ToList());

            //GetReservations
            ViewBag.reservations = db.Reservations.Where(x => x.branchId == branch.id).OrderByDescending(x => x.dateReservedAt).ThenBy(x => x.timeReservedAt);

            if (branch == null)
                return HttpNotFound();

            return View(branch);
        }

        // GET: Branches/Create
        public ActionResult Create()
        {
            var managers = Helper.getMangers(db.Users.ToList(), db);
            ViewBag.restId = new SelectList(db.Resturants, "id", "name");
            ViewBag.userId = new SelectList(managers, "id", "UserName");
            return View();
        }

        // POST: Branches/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,restId,name,userId,address,contactNumber,rating,lat,lon,reservationSpot")] Branch branch)
        {
            var managers = Helper.getMangers(db.Users.ToList(), db);
            ViewBag.restId = new SelectList(db.Resturants, "id", "name");
            ViewBag.userId = new SelectList(managers, "id", "UserName");
            if (ModelState.IsValid)
            {
                if (db.Branches.Where(x => x.name.Replace(" ", "").Trim().ToLower() ==
                    branch.name.Replace(" ", "").Trim().ToLower() && x.restId == branch.restId).Count() == 0)
                {
                    db.Branches.Add(branch);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.error = "Restaurant already exists";
                }
            }

            return View(branch);
        }

        #region Reservations
        public ActionResult EditReservation(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //GetReservations
            var reservation = db.Reservations.Where(x => x.id == id).FirstOrDefault();

            return View(reservation);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReservation([Bind(Include = "id, name, branchId, quantity, dateReservedAt, timeReservedAt, phoneNumber, email, status, comment")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                var updateReservation = db.Reservations.Where(x => x.id == reservation.id).FirstOrDefault();
                var branch = db.Branches.Where(x => x.id == reservation.branchId).FirstOrDefault();

                if (reservation.status == "Awaiting")
                    return View(reservation);

                if (updateReservation.status == reservation.status)
                {
                    //Status has not changed
                    return View(reservation);
                }

                updateReservation.status = reservation.status;
                updateReservation.comment = reservation.comment;
                updateReservation.processedDateTime = DateTime.Now;

                db.Entry(updateReservation).State = EntityState.Modified;
                db.SaveChanges();

                string userEmailAddress = updateReservation.email;
                string reservationSubject = "Reservation Response: " + branch.name;
                string reservationMessage = FormatReservationMessage(updateReservation, branch.name);

                SendEmail(userEmailAddress, reservationSubject, reservationMessage);

                return RedirectToAction("Details", "Branches", new { id = updateReservation.branchId });
            }
            return View(reservation);
        }

        #endregion

        #region Specials
        public ActionResult AddSpecial(int? branchId)
        {
            var special = new BranchMeal() { branchId = Convert.ToInt32(branchId) };
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.branchId = new SelectList(db.Branches, "id", "name");
            ViewBag.branch = db.Branches.First(x => x.id == special.branchId).name;
            return View(special);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSpecial([Bind(Include = "id,branchId,category,name,price,description,startDate,endDate,imgPath")] BranchMeal special, int? branchId)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.branchId = new SelectList(db.Branches, "id", "name");
            ViewBag.branch = db.Branches.First(x => x.id == special.branchId).name;
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, special.imgPath);
                    special.imgPath = randString + ".png";
                    special.branchId = Convert.ToInt32(branchId);
                    db.BranchMeals.Add(special);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(special);
                }
                
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult EditSpecial(int? id)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var branchMeal = db.BranchMeals.Find(id);
            if (branchMeal == null)
            {
                return HttpNotFound();
            }
            return View(branchMeal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSpecial([Bind(Include = "id,name,price,description,category,startDate,endDate,imgPath")] BranchMeal special, int? branchId)
        {
            ViewBag.category = new SelectList(Helper.getCategoryNames(db.Categories.ToList()));
            ViewBag.branchId = new SelectList(db.Branches.ToList(), "id", "name");
            if (ModelState.IsValid)
            {
                string targetPath = Server.MapPath("~");
                var randString = Helper.RandomString(7);
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                
                try
                {
                    Helper.downloadImage(path, special.imgPath);
                    special.imgPath = randString + ".png";
                    special.branchId = Convert.ToInt32(branchId);
                    db.Entry(special).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(special);
                }
                
                return RedirectToAction("Details", "Branches", new { id = branchId });
            }
            return View(special);
        }

        public ActionResult DeleteSpecial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var meal = db.BranchMeals.Find(id);
            if (meal == null)
            {
                return HttpNotFound();
            }
            return View(meal);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("DeleteSpecial")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedSpecial(int id)
        {
            var meal = db.BranchMeals.Find(id);
            db.BranchMeals.Remove(meal);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        #endregion

        // GET: Branches/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branches.Find(id);

            var managers = Helper.getMangers(db.Users.ToList(), db);
            ViewBag.restId = new SelectList(db.Resturants, "id", "name", branch.restId);
            ViewBag.userId = new SelectList(managers, "id", "UserName", branch.userId);

            if (branch == null)
            {
                return HttpNotFound();
            }
            return View(branch);
        }

        // POST: Branches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,restId,userId,name,guid,address,contactNumber,lat,lon,reservationSpot")] Branch branch)
        {
            var managers = Helper.getMangers(db.Users.ToList(), db);
            ViewBag.restId = new SelectList(db.Resturants, "id", "name");
            ViewBag.userId = new SelectList(managers, "id", "UserName");
            if (ModelState.IsValid)
            {
                db.Entry(branch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(branch);
        }

        // GET: Branches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branches.Find(id);
            if (branch == null)
            {
                return HttpNotFound();
            }
            return View(branch);
        }

        // POST: Branches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Branch branch = db.Branches.Find(id);
            var branchMeals = db.BranchMeals.Where(x => x.branchId == branch.id);
            var ratings = db.Ratings.Where(x => x.branchId == branch.id);
            foreach (var item in branchMeals)
            {
                db.BranchMeals.Remove(item);
            }
            foreach (var rating in ratings)
            {
                db.Ratings.Remove(rating);
            }
            db.Branches.Remove(branch);
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

        #region Helpers
        private string FormatReservationMessage(Reservation bookedReservation, string branchName)
        {
            StringBuilder emailBody = new StringBuilder();
            emailBody.AppendLine("Hi " + bookedReservation.name);
            emailBody.AppendLine();

            emailBody.AppendLine(String.Format("Your reservation with {0}, was {1}.", branchName, bookedReservation.status));

            emailBody.AppendLine();

            if (!String.IsNullOrEmpty(bookedReservation.comment))
            {
                emailBody.AppendLine(String.Format("Branch Manager Comment: {0}", bookedReservation.comment));
            }

            emailBody.AppendLine();

            if (bookedReservation.status == "Approved")
            {
                emailBody.AppendLine("See you soon.");
            }
            else if (bookedReservation.status == "Rejected")
                emailBody.AppendLine("Book for a different spot.");

            emailBody.AppendLine();

            emailBody.AppendLine("Thank You,");

            emailBody.AppendLine();

            emailBody.AppendLine("Mail autogenerated by Liger App");

            return emailBody.ToString();
        }

        private void SendEmail(string userEmailAddress, string reservationSubject, string reservationMessage)
        {
            string smtpHost = System.Configuration.ConfigurationManager.AppSettings["smtpHost"];
            int smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["smtpPort"]);
            int timeout = int.Parse(System.Configuration.ConfigurationManager.AppSettings["smtpTimeout"]);

            var systemFromAddress = System.Configuration.ConfigurationManager.AppSettings["fromAddress"];
            var systemFromAddressPassword = System.Configuration.ConfigurationManager.AppSettings["fromAddressPassword"];
            var systemFromAddressName = System.Configuration.ConfigurationManager.AppSettings["fromAddressName"];

            var fromAddress = new MailAddress(systemFromAddress, systemFromAddressName);
            var toAddress = new MailAddress(userEmailAddress);

            string subject = reservationSubject;
            string body = reservationMessage;

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, systemFromAddressPassword),
                Timeout = timeout
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }



        //public string ToFriendlyString(this bool? b)
        //{
        //    if (b is null)
        //        return "Awaiting confirmation";

        //    return (bool)b ? "Approved" : "Declined";
        //}

        //public static MvcHtmlString YesNo(this HtmlHelper htmlHelper, bool? yesNo)
        //{
        //    var text = (bool)yesNo ? "Yes" : "No";
        //    return new MvcHtmlString(text);
        //}

        //Using Google SMTP Client
        //MailMessage mail = new MailMessage();
        //mail.To.Add("sbmusketeer6@gmail.com");
        //mail.From = new MailAddress("sibotage@gmail.com", "Email head", System.Text.Encoding.UTF8);
        //mail.Subject = "Reservation made";
        //mail.SubjectEncoding = System.Text.Encoding.UTF8;
        //mail.Body = "You reservation has been ";// + reservationStatus;
        //mail.BodyEncoding = System.Text.Encoding.UTF8;
        //mail.IsBodyHtml = true;
        //mail.Priority = MailPriority.High;
        //SmtpClient client = new SmtpClient();
        //client.Credentials = new NetworkCredential("sibotage@gmail.com", "fellowcrasher@");
        //client.Port = 587;
        //client.Host = "smtp.gmail.com";
        //client.EnableSsl = true;

        #endregion

    }
}
