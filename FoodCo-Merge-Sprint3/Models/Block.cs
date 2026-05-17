using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public enum EnumBlockStatus
    {
        Unblocked,
        Blocked
    }
    public class Block
    {
        public string BlockerId { get; set; }
        public virtual ApplicationUser Blocker { get; set; }
        public string BlockedId { get; set; }
        public virtual ApplicationUser BlockedUser { get; set; }
        public DateTime BlockDate { get; set; }
        public EnumBlockStatus BlockStatus { get; set; }

        private ApplicationDbContext DB;

        public Block Blocking(string sourceApplicationUserId, string targetApplicationUserId)
        {
            UnBlock(sourceApplicationUserId, targetApplicationUserId, false);
            ApplicationUser sourceApplicationUser = DB.Users.Find(sourceApplicationUserId);
            ApplicationUser targetApplicationUser = DB.Users.Find(targetApplicationUserId);
            if (sourceApplicationUser != null && targetApplicationUser != null && !targetApplicationUser.Blocked)
            {
                Block block = new Block()
                {
                    BlockerId = sourceApplicationUser.Id,
                    BlockedId = targetApplicationUser.Id,
                    BlockStatus = EnumBlockStatus.Blocked,
                    BlockDate = DateTime.Now
                };
                OnLineUsers.AddNotification(targetApplicationUserId, $"Vous avez été bloqué par {sourceApplicationUser.Name} {sourceApplicationUser.Lastname}");
                return block;
            }
            return null;
        }
        public bool UnBlock(string ApplicationUserId_X, string ApplicationUserId_Y, bool notify = true)
        {
            ApplicationUser ApplicationUser_X = DB.Users.Find(ApplicationUserId_X);
            ApplicationUser ApplicationUser_Y = DB.Users.Find(ApplicationUserId_Y);
            if (ApplicationUser_X != null && ApplicationUser_Y != null)
            {
                Block block = DB.Blocks.ToList().Where(f => f.BlockerId == ApplicationUserId_X && f.BlockedId == ApplicationUserId_Y).FirstOrDefault();
                if (block != null)
                {
                    DB.Blocks.Remove(block);
                    DB.SaveChanges();
                }
                else
                {
                    block = DB.Blocks.ToList().Where(f => f.BlockerId == ApplicationUserId_Y && f.BlockedId == ApplicationUserId_X).FirstOrDefault();
                    if (block != null)
                    {
                        DB.Blocks.Remove(block);
                        DB.SaveChanges();
                    }
                }
                if (notify)
                    OnLineUsers.AddNotification(ApplicationUserId_Y, $"{ApplicationUser_X.Name} {ApplicationUser_X.Lastname} vous a débloqué");
            }
            return true;
        }
    }
}