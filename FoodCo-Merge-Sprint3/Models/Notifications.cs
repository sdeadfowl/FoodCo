using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class Notifications
    {
        public string TargetUserId { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}