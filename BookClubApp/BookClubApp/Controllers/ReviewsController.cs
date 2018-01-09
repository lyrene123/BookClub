using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookClubApp.Models;

namespace BookClubApp.Controllers
{
    public class ReviewsController : Controller
    {
        private BookClubDB db = new BookClubDB();

        // GET: Reviews/Create
        [Authorize]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if(book == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookReview = book;       
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "BookId,Rating,Content")] ReviewModel review)
        {
            if (ModelState.IsValid)
            {
                Review userReview = new Review();
                userReview.UserName = User.Identity.Name;
                switch (review?.Rating)
                {
                    case 1: userReview.Rating = -5; break;
                    case 2: userReview.Rating = -3; break;
                    case 3: userReview.Rating = 0; break;
                    case 4: userReview.Rating = 3; break;
                    case 5: userReview.Rating = 5; break;
                    default: userReview.Rating = null; break;
                }
                userReview.BookId = review.BookId;
                userReview.Content = review.Content;
                db.Reviews.Add(userReview);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            Book book = db.Books.Find(review.BookId);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookReview = book;
            return View(review);
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
