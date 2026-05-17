using FoodCo.Models;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

public class PostComment
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }

    public DateTime Date { get; set; }

    public ICollection<AnswerComments> Comments { get; set; }
    public bool isMention { get; set; } = false;

    // Foreign key 
    public int PostId { get; set; }
    public Post Post { get; set; }

    // Foreign key 
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
