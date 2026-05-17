using FoodCo.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public enum EnumRelationStatus
    {
        Follower,
        Following,
        FollowBack,
        UnFollow,
        Friend,
        NotFriend,
        Blocked

        //    NotFriend,
        //    Friend,
        //    Request_Sender,
        //    Request_Receiver,
        //    Have_Decline,
        //    Have_Been_Declined,
        //    Blocked
    }

    public class FollowRepository
    {

        private ApplicationDbContext DB;
        private static string SerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsersSerialNumber"] == null)
                    SetHasChanged();
                return (string)HttpRuntime.Cache["OnLineUsersSerialNumber"];
            }
            set
            {
                HttpRuntime.Cache["OnLineUsersSerialNumber"] = value;
            }
        }
        public bool HasChanged
        {
            get
            {
                string key = this.GetType().Name;
                if (((string)HttpContext.Current.Session[key] != SerialNumber))
                {
                    HttpContext.Current.Session[key] = SerialNumber;
                    return true;
                }
                return false;
            }
        }
        public static void SetHasChanged()
        {
            SerialNumber = Guid.NewGuid().ToString();
        }
        
        public EnumRelationStatus RelationStatus(string sourceApplicationUserId, string targetApplicationUserId)
        {
            ApplicationUser targetApplicationUser = DB.Users.Find(targetApplicationUserId);
            if (targetApplicationUser != null)
            {
                if (targetApplicationUser.Blocked)
                    return EnumRelationStatus.Blocked;
            }
            Follow followOfSourceApplicationUser = DB.Follows.ToList().Where(f => (f.FollowerId == sourceApplicationUserId && f.FollowedUserId == targetApplicationUserId)).FirstOrDefault();
            if (followOfSourceApplicationUser != null)
            {
                if (followOfSourceApplicationUser.FriendshipStatus == EnumFriendshipStatus.Friends)
                    return EnumRelationStatus.Friend;
                if (followOfSourceApplicationUser.FriendshipStatus == EnumFriendshipStatus.UnFollow)
                    return EnumRelationStatus.UnFollow;
                return EnumRelationStatus.Follower;
            }

            Follow followOfTargetApplicationUser = DB.Follows.ToList().Where(f => (f.FollowerId == targetApplicationUserId && f.FollowedUserId == sourceApplicationUserId)).FirstOrDefault();
            if (followOfTargetApplicationUser != null)
            {
                if (followOfTargetApplicationUser.FriendshipStatus == EnumFriendshipStatus.Friends)
                    return EnumRelationStatus.Friend;
                if (followOfTargetApplicationUser.FriendshipStatus == EnumFriendshipStatus.UnFollow)
                    return EnumRelationStatus.UnFollow;
                return EnumRelationStatus.Following;
            }
            return EnumRelationStatus.NotFriend;
        }
    }
}