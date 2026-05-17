using FoodCo.Models;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;

public class RecipeComment
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }

    public DateTime Date { get; set; }
    public bool isMention { get; set; } = false;


    public ICollection<AnswerRecipeComment> Comments { get; set; }

    // Foreign key 
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }

    // Foreign key 
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
