using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class NotificationViewModel
    {
        public string Sender { get; set; }
        public string SenderProfilePicture { get; set; }
        public string SenderName { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
    }

}