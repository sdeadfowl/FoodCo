using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class Admin
    {
        public int Id { get; set; }

        public bool IsAdmin { get; set; }

        //Foreign Key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}