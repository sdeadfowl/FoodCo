using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;

namespace FoodCo.Models
{
    public class Post
    {
        public Post()
        {
            Date = DateTime.Now; 
            IsModified = false;
            Comments = new HashSet<PostComment>();
            Likes = new HashSet<PostLike>();
            Reposts = new HashSet<PostRepost>();
        }

        public int Id { get; set; }

        [Required]
        [Display(Name = "Text")]
        public string Text { get; set; }

        [Display(Name = "Image")]
        public byte[] Content { get; set; }

        [NotMapped]
        [BindNever] 
        public string ContentBase64
        {
            get
            {
                if (Content != null)
                {
                    return Convert.ToBase64String(Content);
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Content = Convert.FromBase64String(value);
                }
            }
        }

        public DateTime Date {  get; set; }
        
        public bool IsModified { get; set; }

        // Foreign key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<PostComment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }
        public ICollection<PostRepost> Reposts { get; set; }
    }
}