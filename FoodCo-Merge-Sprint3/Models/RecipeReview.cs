using FoodCo.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class RecipeReview
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }
    public DateTime Date { get; set; }

    // Foreign key
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }
}
