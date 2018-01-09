using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookClubApp.Models;
using System.Web.Security;

namespace BookClubApp.Controllers
{
    public class AccountController : Controller
    {
        private BookClubDB db = new BookClubDB();
        

        // GET: Account/Create
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "UserName,Password")] User userIn, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = (from u in db.Users
                             where u.UserName.Equals(userIn.UserName)
                             select u).FirstOrDefault<User>();
                if (user != null)
                {
                    if (user.Password.Equals(userIn.Password))                        FormsAuthentication.RedirectFromLoginPage(userIn.UserName, false);
                }
            }
            ViewBag.ReturnUrl = ReturnUrl;
            ModelState.AddModelError("", "Invalid user name or password");
            return View(userIn);
        }

        //GET
        public ActionResult Register()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserName,Password,LastName,FirstName,Email,Country")] User user)
        {
            if (ModelState.IsValid)
            {
                bool contains = false;
                foreach (var i in db.Users)
                {
                    if (i.UserName.Equals(user.UserName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    if (Object.ReferenceEquals(null, user.FirstName))
                        user.FirstName = user.UserName;
                    if (Object.ReferenceEquals(null, user.LastName))
                        user.LastName = "Reader";
                    if (Object.ReferenceEquals(null, user.Country))
                        user.Country = "CAN";

                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "User already exists");
                }
            }         
            return View(user);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
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
