using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string ResponderId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool isModified { get; set; }
        public bool IsRead { get; set; }

        //Foreign Key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


        private static string ChatSerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["ChatSerialNumber"] == null)
                {
                    SetChatHasChanged();
                }
                return (string)HttpRuntime.Cache["ChatSerialNumber"];
            }
            set
            {
                HttpRuntime.Cache["ChatSerialNumber"] = value;
            }
        }

        public static bool ChatHasChanged()
        {
            if (HttpContext.Current.Session["ChatSerialNumber"] == null)
            {
                HttpContext.Current.Session["ChatSerialNumber"] = ChatSerialNumber;
                return true;
            }

            string sessionChatSerialNumber = (string)HttpContext.Current.Session["ChatSerialNumber"];
            HttpContext.Current.Session["ChatSerialNumber"] = ChatSerialNumber;

            return ChatSerialNumber != sessionChatSerialNumber;
        }

        public static void SetChatHasChanged()
        {
            ChatSerialNumber = Guid.NewGuid().ToString(); 
        }
    }
}