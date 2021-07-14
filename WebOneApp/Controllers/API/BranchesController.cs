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
    public class BranchesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Branches
        public List<Branch> GetBranches(string userLocation,int distance)
        {
            var branches = db.Branches.ToList();
            var lon = userLocation.Split(',')[0];
            var lat = userLocation.Split(',')[1];
            var listOfBranches = Helper.GetNearByLocations(lat, lon, Convert.ToInt32(distance), branches); 
            foreach (var branch in listOfBranches)
            {
                Helper.prepareBranch(db, branch);
            }
            return listOfBranches;
        }
        [Route("api/Search")]
        [HttpGet]
        public object getByName(string searchStr) {
            List<Branch> branches = db.Branches.ToList();
            try
            {
                foreach (var branch in branches)
                {
                    Helper.prepareBranch(db,branch);
                }
                var ret = branches.Where(x => x.resturant.name.ToLower().Contains(searchStr.ToLower().Trim())).ToList();
                if (ret.Count() == 0)
                {
                    return new { Errors = "No restaurant found with that name" };
                }

                return ret;
            }
            catch (Exception)
            {
                return new { Errors = "No restaurants found with that name" };
            }
            
        }

        // GET: api/Branches/5
        [ResponseType(typeof(Branch))]
        public object GetBranch(string code)
        {
            try
            {
                Branch branch = db.Branches.ToList().First(x => x.guid == code);
                if (branch == null)
                {
                    return NotFound();
                }
                Helper.prepareBranch(db, branch);
                return Ok(branch);
            }
            catch (Exception)
            {
                return new { Errors = "restaurant not found" };
            }
        }

        // PUT: api/Branches/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBranch(int id, Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != branch.id)
            {
                return BadRequest();
            }

            db.Entry(branch).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BranchExists(id))
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

        // POST: api/Branches
        [ResponseType(typeof(Branch))]
        public IHttpActionResult PostBranch(Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Branches.Add(branch);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = branch.id }, branch);
        }

        #region Reservation
        [Route("api/Reservation")]
        [HttpGet]
        public object AddReservation(int userId, int branchId, int quantity, string mobileNumber, string dateReserved, string timeReserved)
        {
            //https://192.168.0.103:45457/api/Reservation?userId=1002&branchId=9&quantity=2&mobileNumber=0769660476&dateReserved=2021-06-23&timeReserved=20:18

            //Format date and time
            var dateReservedAt = DateTime.Parse(dateReserved);
            var timeReservedAt = DateTime.Parse(timeReserved);

            var user = db.AppUsers.Find(userId);

            //Check if the user already has a reservation
            var userAlreadyMadeBooking = db.Reservations.Where(r => r.branchId == branchId && r.name.Equals(user.Name) && (r.status !="Rejected" && r.status != "Cancelled") && r.dateReservedAt > DateTime.Now); //And not in the past

            if (userAlreadyMadeBooking.Any()) //How may reservations can a user make?
            {
                //User already has either a pending reservation or approved reservation
                return new { error = "Already have a booking" };
            }

            //Check is there are spots available
            var branchSpots = db.Branches.Where(b => b.id == branchId).Select(x => x.reservationSpot).FirstOrDefault();

            var branchReservations = db.Reservations.Where(r => r.branchId == branchId && r.dateReservedAt == dateReservedAt && r.status != "Rejected" || r.status != "Cancelled").Count(); //False is cancelled

            if (branchReservations == branchSpots)
            {
                return new { error = "No reservation spots available for this day" };

            }

            //Spot already approved

            var spotAlreadyBooked = db.Reservations.Where(r => r.branchId == branchId && r.dateReservedAt == dateReservedAt && r.timeReservedAt == timeReservedAt && r.status == "Approved");

            if (spotAlreadyBooked.Any())
            {
                //Spot already booked
                return new { error = "Spot time has already been taken, please select different time " };
            }

            var reservation = new Reservation()
            {
                name = user.Name,
                branchId = branchId,
                quantity = quantity,
                phoneNumber = mobileNumber,
                email = user.Email,
                dateReservedAt = dateReservedAt,
                timeReservedAt = timeReservedAt,
                status = "Awaiting",
                processedDateTime = DateTime.Now,
            };

            db.Reservations.Add(reservation);
            db.SaveChanges();
            return reservation;
        }


        #endregion
        // DELETE: api/Branches/5
        [ResponseType(typeof(Branch))]
        public IHttpActionResult DeleteBranch(int id)
        {
            Branch branch = db.Branches.Find(id);
            if (branch == null)
            {
                return NotFound();
            }

            db.Branches.Remove(branch);
            db.SaveChanges();

            return Ok(branch);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BranchExists(int id)
        {
            return db.Branches.Count(e => e.id == id) > 0;
        }
    }
}