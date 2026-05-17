using FoodCo.Models;
using System;

public class PostRepost
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    // Foreign key
    public int PostId { get; set; }
    public Post Post { get; set; }

    // Foreign key
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
