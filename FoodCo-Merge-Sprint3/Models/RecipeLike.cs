using FoodCo.Models;

public class RecipeLike
{
    public int Id { get; set; }

    // Foreign key 
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }

    // Foreign key 
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
