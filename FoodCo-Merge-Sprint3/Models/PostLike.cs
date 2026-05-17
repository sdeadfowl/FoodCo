using FoodCo.Models;

public class PostLike
{
    public int Id { get; set; }

    // Foreign key 
    public int PostId { get; set; }
    public Post Post { get; set; }

    // Foreign key 
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
