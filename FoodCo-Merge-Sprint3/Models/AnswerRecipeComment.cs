using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class AnswerRecipeComment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime Date { get; set; }

        // Foreign key 
        public int recipeCommentId { get; set; }
        public RecipeComment RecipeComment { get; set; }

        // Foreign key 
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

       
    }
}