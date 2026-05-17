using FoodCo.Models;
using System;

public class RecipeRepost
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    // Foreign key
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }

    // Foreign key
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
