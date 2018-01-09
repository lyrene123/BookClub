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
    public class HomeController : Controller
    {
        private BookClubDB db = new BookClubDB();

        // GET: Home
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated) //The following code for user who signed in
            {
                User user = db.Users.Find(User.Identity.Name); //find user who logged in
                if (user == null)
                {
                    return HttpNotFound();
                }

                //get reviews done by the user who logged in
                List<Review> userReviews = user.Reviews.ToList();

                //if user has not done any reviews yet, get top rated books instead
                if (userReviews.Count() == 0)
                {
                    List<Book> topTenRatedBooks = getTopRatedBooks();
                    topTenRatedBooks = topTenRatedBooks.OrderByDescending(o => o.Reviews.Count()).ToList();
                    return View(topTenRatedBooks); //already ordered descending
                }
                else
                {
                    List<Book> recommended = getRecommendedList(user, userReviews);
                    return View(recommended.OrderByDescending(o => o.Reviews.Count()).ToList());
                }
            }
            else //if not user authenticated
            {
                List<Book> allBooks = db.Books.ToList();
                List<Book> sortedBooks = (allBooks.OrderByDescending(o => o.Reviews.Count())).ToList();
                List<Book> topTen = sortedBooks.GetRange(0, 10);
                return View(topTen);
            }     
        }



        /// <summary>
        /// The getRecommendedList method will return a list of recommended books based
        /// on a user and list of reviews of that input
        /// </summary>
        /// <param name="user">A user</param>
        /// <param name="userReviews">A list of reviews</param>
        /// <returns></returns>
        private List<Book> getRecommendedList(User user, List<Review> userReviews)
        {
            //get all user object from database except user who logged in
            List<User> allUsers = (from u in db.Users
                                   where u.UserName != user.UserName
                                   select u).ToList();

            //list that will contain all dot products
            List<int?> products = new List<int?>();

            //loop through all users in the database
            foreach (var aUser in allUsers)
            {
                //list of ratings made by iterated user
                List<int?> ratings = new List<int?>();

                //for each iterated user, loop through the reviews of the user who logged in
                foreach (var aReview in userReviews)
                {
                    //get the user's review to the same book 
                    Review r = (from item in aUser.Reviews
                                where item.BookId == aReview.BookId
                                select item).FirstOrDefault(); //can be null if no review found
                    ratings.Add(r?.Rating); //rating can also be null if user has not given a rating
                }

                //calculate the dot product for that user
                int? dotProduct = 0;
                bool isChanged = false;
                for (int i = 0; i < ratings.Count(); i++)
                {
                    if (ratings[i] != null && userReviews[i]?.Rating != null)
                    {
                        dotProduct += ratings[i] * userReviews[i]?.Rating;
                        isChanged = true;
                    }
                }


                //put into list the dotProduct
                if (isChanged)
                {
                    products.Add(dotProduct);
                }
                else
                {
                    products.Add(null);
                }
            }


            List<Book> recommended = new List<Book>();

            while (recommended.Count() < 10)
            {
                int index = products.FindIndex(max => max == products.Max());
                User currentUser = allUsers[index];
                int? maxRating = currentUser.Reviews.Max(m => m?.Rating);
                List<Book> topBooks = (from top in currentUser.Reviews
                                       where top?.Rating == maxRating
                                       select top.Book).ToList();

                //list of book from topBooks list that user has not read yet
                List<Book> unrated = new List<Book>();
                foreach (var bestBook in topBooks)
                {
                    Review r = (from userrev in userReviews
                                where userrev.BookId == bestBook.BookId
                                select userrev).FirstOrDefault();

                    Book b = (from recom in recommended
                              where recom.BookId == bestBook.BookId
                              select recom).FirstOrDefault();

                    //if user who logged in has not rated one of the recommended books, 
                    //and the book has not yet been added to the recommended books list,
                    //then add it to the unrated list
                    if (Object.ReferenceEquals(r, null) && (Object.ReferenceEquals(b, null)))
                    {
                        unrated.Add(bestBook);
                    }
                }

                //get 10 top recommended books
                if (unrated.Count() > 0 && unrated.Count() <= 10 && recommended.Count() == 0)
                {
                    recommended.AddRange(unrated);
                }
                else
                {
                    if (unrated.Count() != 0)
                    {
                        int spaceLeft = 10 - recommended.Count();
                        if (spaceLeft <= unrated.Count())
                        {
                            recommended.AddRange(unrated.GetRange(0, spaceLeft));
                        }
                        else
                        {
                            recommended.AddRange(unrated);
                        }
                    }
                }
                //remove the highest dot product in order to find the next highest product
                products.Remove(products[index]);
            }
            return recommended;
        }




        /// <summary>
        /// The getTopRatedBooks method returns the top 10 rated books
        /// </summary>
        /// <returns>List of type books of top rated books</returns>
        private List<Book> getTopRatedBooks()
        {
            //get all books from db
            List<Book> allBooks = db.Books.ToList();
            List<ViewModel> vm = new List<ViewModel>(); //create a viewmodel list

            //loop through each books from the db and "convert" it to a viewmodel object
            foreach(var bookItem in allBooks)
            {
                ViewModel bookView = new ViewModel();
                bookView.Book = bookItem; //add book to viewmodel

                //calculate the avg rating for that book
                double sum = 0;
                foreach (var review in bookItem.Reviews)
                {
                    switch (review?.Rating)
                    {
                        case -5: sum += 1; break;
                        case -3: sum += 2; break;
                        case 0: sum += 3; break;
                        case 3: sum += 4; break;
                        case 5: sum += 5; break;
                        default: sum += 0; break;
                    }
                }
                bookView.Average = Math.Round(sum / bookItem.Reviews.Count(), 2);
                vm.Add(bookView); //add avg rating 
            }

            //sort books by average rating by descending
            List<ViewModel> sortedBooks = (vm.OrderByDescending(r => r.Average)).ToList();
            //get only the top ten rated books
            List<Book> topTen = new List<Book>();
            for(int i = 0; i<10; i++)
            {
                topTen.Add(sortedBooks[i].Book);
            }
            return topTen;
        }





        public ActionResult AuthorDetails(int? id, string returnUrl)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorName = author.FirstName + " " + author.LastName;
            ViewBag.ReturnURL = returnUrl;
            List<Book> books = author.Books.ToList().OrderByDescending(o => o.Reviews.Count()).ToList();
            return View(books);
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            if (book.Views == null)
                book.Views = 1;
            else
                book.Views++;

            db.SaveChanges();

            ViewModel bookView = new ViewModel();
            bookView.Book = book;

            double sum = 0;
            foreach (var review in book.Reviews)
            {
                switch (review?.Rating)
                {
                    case -5: sum += 1; break;
                    case -3: sum += 2; break;
                    case 0: sum += 3; break;
                    case 3: sum += 4; break;
                    case 5: sum += 5; break;
                    default: sum += 0; break;
                }
            }
            bookView.Average = Math.Round(sum / book.Reviews.Count(), 2);
            return View(bookView);
        }


        // GET: Home/Create
        //The following is for the creation of books
        [Authorize]
        public ActionResult Create()
        {
            var selectList = new List<SelectListItem>();
            foreach (var author in db.Authors)
            {
                selectList.Add(new SelectListItem
                {
                    Value = author.AuthorId.ToString(),
                    Text = author.FirstName + " " +author.LastName
                });
            }
            ViewBag.Author1 = selectList;
            ViewBag.Author2 = selectList;
            return View();
        }



        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //The following is for the creation of books
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Title,Description")] Book book, string Author1, string Author2)
        {
            if (ModelState.IsValid)
            {
                if((Author1.Equals("") && Author2.Equals("")) || Author1.Equals(Author2, StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("", "Please choose two unique authors or at least one author");
                }
                else
                {
                    Author a1 = null;
                    Author a2 = null;
                    var authorsList = db.Authors.ToList();
                    bool match1 = false;
                    bool match2 = false;

                    if (!Author1.Equals(""))
                    {
                        match1 = checkAuthor1(book, Author1);
                        int index1 = Convert.ToInt32(Author1);
                        a1 = db.Authors.Find(index1);
                    }
                        
                    if(!Author2.Equals(""))
                    {
                        match2 = checkAuthor2(book, Author2);
                        int index2 = Convert.ToInt32(Author2);
                        a2 = db.Authors.Find(index2);
                    }

                    bool error = checkAuthorErrors(Author1,Author2,match1,match2);
                    
                    //if no error has occured, than save to database
                    if (!error)
                    {
                        if (!Author1.Equals(""))
                            book.Authors.Add(a1);
                        if (!Author2.Equals(""))
                            book.Authors.Add(a2);
                        if (Object.ReferenceEquals(book.Description, null) || book.Description.Equals(""))
                            book.Description = "No description available";

                        db.Books.Add(book);
                        db.SaveChanges();
                        return RedirectToAction("Index", "Home");
                    }                                       
                }  
            }

            //return once again the viewbag
            var selectList = new List<SelectListItem>();
            foreach (var author in db.Authors)
            {
                selectList.Add(new SelectListItem
                {
                    Value = author.AuthorId.ToString(),
                    Text = author.FirstName + " " + author.LastName
                });
            }
            ViewBag.Author1 = selectList;
            ViewBag.Author2 = selectList;
            return View(book);
        }



        /// <summary>
        /// checkAuthorErrors method will take care of letting know the user any errors
        /// relating to the authors chosen for adding books
        /// </summary>
        /// <param name="Author1">string author id for author 1</param>
        /// <param name="Author2">string author id for author 2</param>
        /// <param name="match1">boolean true or false if author 1 has a match in db</param>
        /// <param name="match2">boolean true or false if author 2 has a match in db</param>
        /// <returns></returns>
        private bool checkAuthorErrors(string Author1, string Author2, bool match1, bool match2)
        {
            bool error = false;
           
            //if author1 not empty and author2 is empty
            if (!Author1.Equals("") && Author2.Equals(""))
            {
                if (match1)
                {
                    ModelState.AddModelError("", "The book you are trying to add already exists.");
                    error = true;
                }
            }

            //if author2 not empty and author1 is empty
            if (!Author2.Equals("") && Author1.Equals(""))
            {
                if (match2)
                {
                    ModelState.AddModelError("", "The book you are trying to add already exists.");
                    error = true;
                }
            }

            //if both authors not empty
            if (!Author1.Equals("") && !Author2.Equals(""))
            {
                int index2 = Convert.ToInt32(Author2);
                Author a2 = db.Authors.Find(index2);
                int index1 = Convert.ToInt32(Author1);
                Author a1 = db.Authors.Find(index1);

                //if both authors have that book already
                if (match1 && match2)
                {
                    ModelState.AddModelError("", "The book you are trying to add already exists.");
                    error = true;
                }
                else
                {
                    //if only 1st author has already that book, specify it to the user
                    if (match1)
                    {
                        ModelState.AddModelError("", "The book you are trying to add already exists for " + a1.FirstName + " " + a1.LastName);
                        error = true;
                    }
                    //if only 2nd author has already that book, specify it to the user
                    if (match2)
                    {
                        ModelState.AddModelError("", "The book you are trying to add already exists for " + a2.FirstName + " " + a2.LastName);
                        error = true;
                    }
                }
            }
            return error;
        }



        /// <summary>
        /// checkAuthor2 method will validate the user's choice in author2 drop down list
        /// when creating new book 
        /// </summary>
        /// <param name="book">A book</param>
        /// <param name="Author1">An author id</param>
        /// <returns></returns>
        private bool checkAuthor2(Book book, string Author2)
        {
            var authorsList = db.Authors.ToList();
            int index2 = Convert.ToInt32(Author2);
            Author a2 = db.Authors.Find(index2);
            var match2 = false;

            foreach (var a in authorsList)
            {
                bool isNullFirst = false;
                bool isNullLast = false;
                if (a2.FirstName == null)
                    isNullFirst = true;
                if (a2.LastName == null)
                    isNullLast = false;


                if (!isNullLast && !isNullFirst)
                {
                    if (a2.FirstName.Equals(a?.FirstName) && a2.LastName.Equals(a?.LastName))
                    {
                        var booksAuthor = a.Books.ToList();
                        foreach (var b in booksAuthor)
                        {
                            if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                            {
                                match2 = true;
                                break;
                            }
                        }
                        if (match2)
                            break;
                    }
                }
                else
                {
                    //if first name is the one that's null, then compare only last name
                    if (isNullFirst && !isNullLast)
                    {
                        if (a2.LastName.Equals(a?.LastName))
                        {
                            var booksAuthor = a.Books.ToList();
                            foreach (var b in booksAuthor)
                            {
                                if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    match2 = true;
                                    break;
                                }
                            }
                            if (match2)
                                break;
                        }
                    }

                    //if it's the last name that's null, then compare only first name
                    if (!isNullFirst && isNullLast)
                    {
                        if (a2.LastName.Equals(a?.LastName))
                        {
                            var booksAuthor = a.Books.ToList();
                            foreach (var b in booksAuthor)
                            {
                                if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    match2 = true;
                                    break;
                                }
                            }
                            if (match2)
                                break;
                        }
                    }
                }
            }
            return match2;
        }



        /// <summary>
        /// checkAuthor1 method will validate the user's choice in author1 drop down list
        /// when creating new book 
        /// </summary>
        /// <param name="book">A book</param>
        /// <param name="Author1">An author id</param>
        /// <returns></returns>
        private bool checkAuthor1(Book book, string Author1)
        {
            var authorsList = db.Authors.ToList();
            int index1 = Convert.ToInt32(Author1);
            Author a1 = db.Authors.Find(index1);
            var match1 = false;

            foreach (var a in authorsList)
            {
                bool isNullFirst = false;
                bool isNullLast = false;
                if (a1.FirstName == null)
                    isNullFirst = true;
                if (a1.LastName == null)
                    isNullLast = false;

                if (!isNullLast && !isNullFirst)
                {
                    if (a1.FirstName.Equals(a?.FirstName) && a1.LastName.Equals(a?.LastName))
                    {
                        var booksAuthor = a.Books.ToList();
                        foreach (var b in booksAuthor)
                        {
                            if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                            {
                                match1 = true;
                                break;
                            }
                        }

                        if (match1)
                            break;
                    }
                }
                else
                {
                    //if first name is the one that's null, then compare only last name
                    if (isNullFirst && !isNullLast)
                    {
                        if (a1.LastName.Equals(a?.LastName))
                        {
                            var booksAuthor = a.Books.ToList();
                            foreach (var b in booksAuthor)
                            {
                                if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    match1 = true;
                                    break;
                                }
                            }
                            if (match1)
                                break;
                        }
                    }

                    //if it's the last name that's null, then compare only first name
                    if (!isNullFirst && isNullLast)
                    {
                        if (a1.LastName.Equals(a?.LastName))
                        {
                            var booksAuthor = a.Books.ToList();
                            foreach (var b in booksAuthor)
                            {
                                if (b.Title.Equals(book.Title, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    match1 = true;
                                    break;
                                }
                            }
                            if (match1)
                                break;
                        }
                    }
                }
            }
            return match1;
        }






        [Authorize]
        //GET: Home/CreateAuthor
        public ActionResult CreateAuthor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        //POST: Home/CreateAuthor
        public ActionResult CreateAuthor([Bind(Include = "LastName,FirstName")] Author author)
        {
            if (ModelState.IsValid)
            {              
                bool sameFname = false;
                bool sameLname = false;

                foreach (var a in db.Authors)
                {
                    var fname = a.FirstName;
                    var lname = a.LastName;

                    //compare first name
                    if(Object.ReferenceEquals(fname, null) && Object.ReferenceEquals(author.FirstName, null))
                    {
                        sameFname = true;
                    }
                    else
                    {
                        if(!(Object.ReferenceEquals(fname, null)))
                        {
                            if (fname.Equals(author.FirstName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                sameFname = true;
                            }
                        }
                    }


                    //compare last name
                    if (Object.ReferenceEquals(lname, null) && Object.ReferenceEquals(author.LastName, null))
                    {
                        sameLname = true;
                    }
                    else
                    {
                        if (!(Object.ReferenceEquals(lname, null)))
                        {
                            if (lname.Equals(author.LastName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                sameLname = true;
                            }
                        }
                    }

                    if (sameLname && sameFname)
                    {
                        break;
                    }
                    else
                    {
                        sameFname = false;
                        sameLname = false;
                    }                                       
                }

                if (!sameLname && !sameFname)
                {
                    db.Authors.Add(author);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Author already exists");
                }

             }
            return View(author);
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
