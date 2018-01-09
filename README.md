# BookClub
C# ASP.NET MVC website, Project 3, 2nd Year CompSci.<br>
An online book club with recommendations based on users with similar tastes.

## Features
* When anyone connects to the site, a summary (index) page is presented, which consists of a list of the books, 
displayed by either number of reviews or views
* A user must log in, in order to add a new book / review an existing book. 
If a user is logged in, the index now shows a list of books recommended for the user based on a recommendation algorithm.
* A link associated with each book on the summary page directs to a details page: the book title, author(s) 
and a description is shown, along with the average rating and all the reviews. 
The author(s) are links that lead to a page with all books by the author.
* A user can register: can add a new author, can add a new book, can review a book.

This project was completed in two major parts: Console application and MVC Web Application
The console application consists of the database design and loading database with data read from an xml file.
