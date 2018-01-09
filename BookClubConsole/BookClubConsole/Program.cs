using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookClubConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {

            /**
             * NOTE: When filling up the database after multiple failed times (haha), I finally
             *  was able to avoid making mistakes. And so, for my final try, I decided to use the 
             *  bookid attribute of the books xml object as id when adding the books in the database. 
             *  This helped me avoid having duplicates of books in the Book Table. 
             * */


            //load the books.xml file
            XElement bookXml = XElement.Load("books.xml");

            //create all Author objects, setting the name properties
            List<Author> authors = (from a in bookXml.Descendants("author")
                                    select new Author
                                    {
                                        LastName = a.Attribute("lastName")?.Value,
                                        FirstName = a.Attribute("firstName")?.Value
                                    }).ToList();

            //create anonymous objects
            var anonbooks = (from anon in bookXml.Descendants("book")
                             select new
                             {
                                 Title = anon.Element("title").Value,
                                 Description = anon.Element("description").Value,
                                 FirstName = anon.Element("author").Attribute("firstName")?.Value,
                                 LastName = anon.Element("author").Attribute("lastName")?.Value
                             }).ToList();

            //create the books objects
            List<Book> books = new List<Book>();
            foreach (var bookitem in anonbooks)
            {

                Book aBook = new Book
                {
                    Title = bookitem.Title,
                    Description = bookitem.Description
                };

                Author auth = authors.Where(a => a.FirstName == bookitem.FirstName
                                               && a.LastName == bookitem.LastName).FirstOrDefault();
                auth?.Books.Add(aBook);
                books.Add(aBook);
            }

            //add authors to the database
            // addAuthors(authors);



            //load the ratings.xml file
            XElement ratingXml = XElement.Load("ratings.xml");

            //create the user objects
            List<User> users = (from u in ratingXml.Descendants("user")
                                select new User
                                {
                                    UserName = u.Attribute("userId").Value,
                                    Password = u.Attribute("userId").Value,
                                    LastName = u.Attribute("lastName")?.Value,
                                    FirstName = u.Attribute("userId")?.Value,
                                    Email = u.Attribute("email")?.Value,
                                    Country = u.Attribute("country")?.Value
                                }).ToList();

            //add the users to the database
            //addUsers(users);




            //get all users from XML file first
            List<XElement> usersXML = (from ux in ratingXml.Descendants("user")
                                       select ux).ToList();

            List<Review> reviews = new List<Review>();
            //loop through each item in usersXML and get each review
            foreach (var useritem in usersXML)
            {
                //get the reviews of current useritem in the itteration 
                List<XElement> userrev = (from rev in useritem.Elements("review")
                                          select rev).ToList();

                //loop through each review
                foreach (var itemrev in userrev)
                {
                    int index = Convert.ToInt32(itemrev.Attribute("bookId").Value);
                    Book b = books[index]; //get the book reviewed
                    //create the review objects
                    Review rev = new Review
                    {
                        UserName = useritem.Attribute("userId").Value,
                        Rating = Convert.ToInt32(itemrev.Attribute("rating")?.Value),
                        Content = useritem.Attribute("content")?.Value,
                        BookId = index
                    };
                    reviews.Add(rev); //add review to the list
                }
            }

           // addReviews(reviews);

        }



        public static void addAuthors(List<Author> authors)
        {

            Console.WriteLine("adding authors to the database");
            using (var db = new BookClubDB())
            {
                foreach (var a in authors)
                {
                    db.Authors.Add(a);
                    try
                    {
                        db.SaveChanges();
                        Console.WriteLine("author " + a.FirstName + " " + a.LastName + " added");

                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        Console.WriteLine("Duplicate found");
                        db.Authors.Remove(a);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void addUsers(List<User> users)
        {
            using (var db = new BookClubDB())
            {
                foreach (var u in users)
                {
                    db.Users.Add(u);
                    try
                    {
                        db.SaveChanges();
                        Console.WriteLine("User " + u.FirstName + " " + u.LastName + " added");

                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        Console.WriteLine("Duplicate found");
                        db.Users.Remove(u);
                        db.SaveChanges();
                    }
                }

            }
        }

        public static void addReviews(List<Review> reviews)
        {
            using (var db = new BookClubDB())
            {
                foreach (var r in reviews)
                {
                    db.Reviews.Add(r);
                    try
                    {
                        db.SaveChanges();
                        Console.WriteLine("Review of " + r.UserName + " added");

                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        Console.WriteLine("Duplicate found");
                        db.Reviews.Remove(r);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
