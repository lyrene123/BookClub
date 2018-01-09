using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookClubApp.Models
{
    public class ReviewModel
    {
        public ReviewModel()
        {

        }

        public int ReviewId { get; set; }
        public int BookId { get; set; }

        public string UserName { get; set; }

        [Display(Name = "How would you rate this book from 1 - 5 stars?")]
        [Range(1, 5, ErrorMessage = "Rating must be between {1} to {2} stars")]
        public Nullable<int> Rating { get; set; }

        [Display(Name = "Leave a comment!")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Please enter at least a comment")]
        public string Content { get; set; }

    }
}