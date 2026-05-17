using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodCo.Models
{
    public enum EnumFriendshipStatus
    {
        Following,
        Followed,
        UnFollow,
        Friends,
        Blocked
    }
    public class Follow
    {
        public string Id { get; set; }
        public string FollowerId { get; set; }
        public virtual ApplicationUser Follower { get; set; }

        public string FollowedUserId { get; set; }
        public virtual ApplicationUser FollowedUser { get; set; }
        public DateTime CreationDate { get; set; }
        public EnumFriendshipStatus FriendshipStatus { get; set; }


        private ApplicationDbContext DB;
        public Follow Following(string sourceApplicationUserId, string targetApplicationUserId)
        {
            Unfollow(sourceApplicationUserId, targetApplicationUserId, false);
            ApplicationUser sourceApplicationUser = DB.Users.Find(sourceApplicationUserId);
            ApplicationUser targetApplicationUser = DB.Users.Find(targetApplicationUserId);
            if (sourceApplicationUser != null && targetApplicationUser != null && !targetApplicationUser.Blocked)
            {
                Follow follow = new Follow()
                {
                    FollowerId = sourceApplicationUser.Id,
                    FollowedUserId = targetApplicationUser.Id,
                    FriendshipStatus = EnumFriendshipStatus.Following,
                    CreationDate = DateTime.Now
                };
                OnLineUsers.AddNotification(targetApplicationUserId, $"Vous avez reçu une demande d'amitié de {sourceApplicationUser.Name} {sourceApplicationUser.Lastname}");
                return follow;
            }
            return null;
        }
        public bool Unfollow(string ApplicationUserId_X, string ApplicationUserId_Y, bool notify = true)
        {
            ApplicationUser ApplicationUser_X = DB.Users.Find(ApplicationUserId_X);
            ApplicationUser ApplicationUser_Y = DB.Users.Find(ApplicationUserId_Y);
            if (ApplicationUser_X != null && ApplicationUser_Y != null)
            {
                Follow follow = DB.Follows.ToList().Where(f => f.FollowerId == ApplicationUserId_X && f.FollowedUserId == ApplicationUserId_Y).FirstOrDefault();
                if (follow != null)
                {
                    DB.Follows.Remove(follow);
                    DB.SaveChanges();
                }
                else
                {
                    follow = DB.Follows.ToList().Where(f => f.FollowerId == ApplicationUserId_Y && f.FollowedUserId == ApplicationUserId_X).FirstOrDefault();
                    if (follow != null)
                    {
                        DB.Follows.Remove(follow);
                        DB.SaveChanges();
                    }
                }
                if (notify)
                    OnLineUsers.AddNotification(ApplicationUserId_Y, $"{ApplicationUser_X.Name} {ApplicationUser_X.Lastname} a retiré de votre amitié");
            }
            return true;
        }
        public bool Follow_Back(string targetApplicationUserId, string sourceApplicationUserId)
        {
            Follow follow = DB.Follows.ToList().Where(f => (f.FollowerId == targetApplicationUserId && f.FollowedUserId == sourceApplicationUserId)).FirstOrDefault();
            if (follow != null)
            {
                follow.FriendshipStatus = EnumFriendshipStatus.Friends;
                ApplicationUser sourceApplicationUser = DB.Users.Find(sourceApplicationUserId);
                ApplicationUser targetApplicationUser = DB.Users.Find(targetApplicationUserId);
                OnLineUsers.AddNotification(targetApplicationUserId, $"{sourceApplicationUser.Name} a accepté votre demande d'amitié");
                return true;
            }
            return false;
        }

        public bool FriendAccepted(string sourceApplicationUserId, string targetApplicationUserId)
        {
            ApplicationUser targetApplicationUser = DB.Users.Find(targetApplicationUserId);
            if (targetApplicationUser != null)
            {
                if (targetApplicationUser.Blocked)
                    return false;
            }
            else
                return false;
            ApplicationUser sourceApplicationUser = DB.Users.Find(sourceApplicationUserId);
            if (sourceApplicationUser != null)
            {
                if (sourceApplicationUser.Blocked)
                    return false;
            }
            else
                return false;

            Follow follow = DB.Follows.ToList().Where(f => (f.FollowerId == sourceApplicationUserId && f.FollowedUserId == targetApplicationUserId)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.Friends;
            }
            follow = DB.Follows.ToList().Where(f => (f.FollowerId == targetApplicationUserId && f.FollowedUserId == sourceApplicationUserId)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.Friends;
            }
            return false;
        }
        public bool AreFriends(string ApplicationUserId_X, string ApplicationUserId_Y)
        {
            ApplicationUser ApplicationUser_X = DB.Users.Find(ApplicationUserId_X);
            if (ApplicationUser_X != null)
            {
                if (ApplicationUser_X.Blocked)
                    return false;
            }
            else
                return false;

            ApplicationUser ApplicationUser_Y = DB.Users.Find(ApplicationUserId_Y);
            if (ApplicationUser_Y != null)
            {
                if (ApplicationUser_Y.Blocked)
                    return false;
            }
            else
                return false;

            Follow follow = DB.Follows.ToList().Where(f => (f.FollowerId == ApplicationUserId_X && f.FollowedUserId == ApplicationUserId_Y)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.Friends;
            }
            follow = DB.Follows.ToList().Where(f => (f.FollowerId == ApplicationUserId_Y && f.FollowedUserId == ApplicationUserId_X)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.Friends;
            }
            return false;
        }
        public bool HaveFriendRelation(string ApplicationUserId_X, string ApplicationUserid_Y)
        {
            Follow follow = DB.Follows.ToList().Where(f => (f.FollowerId == ApplicationUserId_X && f.FollowedUserId == ApplicationUserid_Y)).FirstOrDefault();
            if (follow != null) return true;
            else
            {
                follow = DB.Follows.ToList().Where(f => (f.FollowerId == ApplicationUserid_Y && f.FollowedUserId == ApplicationUserId_X)).FirstOrDefault();
                if (follow != null) return true;
            }
            return false;
        }
        public bool UnfollowRequest(string ApplicationUserId_X, string ApplicationUserId_Y)
        {
            ApplicationUser ApplicationUser_X = DB.Users.Find(ApplicationUserId_X);
            ApplicationUser ApplicationUser_Y = DB.Users.Find(ApplicationUserId_Y);
            if (ApplicationUser_X != null && ApplicationUser_Y != null)
            {
                Follow follow = DB.Follows.ToList().Where(f => f.FollowerId == ApplicationUserId_X && f.FollowedUserId == ApplicationUserId_Y).FirstOrDefault();
                if (follow != null)
                {
                    DB.Follows.Remove(follow);
                    DB.SaveChanges();
                }
                else
                {
                    follow = DB.Follows.ToList().Where(f => f.FollowerId == ApplicationUserId_Y && f.FollowedUserId == ApplicationUserId_X).FirstOrDefault();
                    if (follow != null)
                    {
                        DB.Follows.Remove(follow);
                        DB.SaveChanges();
                    }
                }
                OnLineUsers.AddNotification(ApplicationUserId_Y, $"{ApplicationUser_X.Name} a retiré sa demande amitié");
            }
            return true;
        }


        public bool followDeclined(string sourceApplicationUserId, string targetApplicationUserId)
        {
            Follow follow = DB.Follows.ToList().Where(f => (f.FollowerId == sourceApplicationUserId && f.FollowedUserId == targetApplicationUserId)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.UnFollow;
            }
            follow = DB.Follows.ToList().Where(f => (f.FollowerId == targetApplicationUserId && f.FollowedUserId == sourceApplicationUserId)).FirstOrDefault();
            if (follow != null)
            {
                return follow.FriendshipStatus == EnumFriendshipStatus.UnFollow;
            }
            return false;
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
