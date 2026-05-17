using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System;

namespace FoodCo.Models
{
    public class Recipe
    {
        public Recipe()
        {
            Date = DateTime.Now;
            IsModified = false;
            Comments = new HashSet<RecipeComment>();
            Likes = new HashSet<RecipeLike>();
            Reposts = new HashSet<RecipeRepost>();
            Reviews = new HashSet<RecipeReview>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la recette est requis.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Content")]
        public string ContentPath { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Ingrédients")]
        public ICollection<RecipeIngredient> Ingredients { get; set; }

        [Required]
        [Display(Name = "Étapes")]
        public ICollection<RecipeStep> RecipeSteps { get; set; }

        [Required]
        [Display(Name = "Tag")]
        public string Tag { get; set; }

        public bool IsModified { get; set; }

        // Foreign key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<RecipeComment> Comments { get; set; }
        public ICollection<RecipeLike> Likes { get; set; }
        public ICollection<RecipeRepost> Reposts { get; set; }
        public virtual ICollection<RecipeReview> Reviews { get; set; }

        public double AverageRating()
        {
            if (Reviews == null || Reviews.Count == 0) return 0;
            return Reviews.Average(r => r.Stars);
        }

        public int ReviewCount()
        {
            if (Reviews == null) return 0;
            return Reviews.Count;
        }

    }
}