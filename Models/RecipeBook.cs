using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class RecipeBook
    {
        public int Id { get; set; }

        //Foreign Key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        //Foreign Key

        public int RecipeId { get; set; }
        public Recipe recipes { get; set; }
    }
}