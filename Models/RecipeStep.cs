using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;

namespace FoodCo.Models
{
    public class RecipeStep
    {
        //public RecipeIngredient()
        //{
        //    Date = DateTime.Now;
        //    IsModified = false;
        //    //Comments = new HashSet<PostComment>();
        //    //Likes = new HashSet<PostLike>();
        //    //Reposts = new HashSet<PostRepost>();
        //}

        public int Id { get; set; }

        [Required]
        [Display(Name = "Step number")]
        public string StepNumber { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        // Foreign key
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
