using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FoodCo.Models
{
    public class AdminRepository : IdentityUser
    {
        private ApplicationDbContext DB;

        public AdminRepository(ApplicationDbContext context)
        {
            DB = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Email == email);

            return user;
        }

        public ApplicationUser Get(string id)
        {
            ApplicationUser user = DB.Users.FirstOrDefault(u => u.Id == id);

            return user;
        }

        public async Task<ApplicationUser> GetAsync(string id)
        {
            ApplicationUser user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }


        public async Task<string> GetUsernameAsync(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);
            string username = user.UserName;

            return username;
        }

        public async Task<string> GetProfilePictureAsync(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);
            string profilePicture = user.ProfilePicturePath;

            return profilePicture;
        }

        public async Task<string> GetNameAsync(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);
            string name = user.Name;

            return name;
        }
        public async Task<string> GetFullNameAsync(string id)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.Id == id);
            string fullname = user.Name + " " + user.Lastname;

            return fullname;
        }

        public async Task<ApplicationUser> GetUserByUsername(string username)
        {
            var user = await DB.Users.FirstOrDefaultAsync(u => u.UserName == username);

            return user;
        }


        public string GetUserIdByUsername(string username)
        {
            ApplicationUser user = DB.Users.Where(u => u.UserName == username).FirstOrDefault();
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                return null;
            }
        }
        public string GetUserIdByEmail(string email)
        {
            var user = DB.Users.Where(u => u.Email == email).FirstOrDefault();
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                return null;
            }
        }
        public string GetFullName(string Id)
        {
            var user = DB.Users.Where(u => u.Id == Id).FirstOrDefault();
            if (user != null)
            {
                return $"{user.Name} {user.Lastname}";
            }
            else
            {
                return null;
            }
        }

        public string GetUserEmail(string id)
        {
            var user = DB.Users.Where(u => u.Id == id).FirstOrDefault();
            if (user != null)
            {
                return user.Email;
            }
            else
            {
                return null;
            }
        }
        public string GetUsername(string id)
        {
            var user = DB.Users.Where(u => u.Id == id).FirstOrDefault();
            if (user != null)
            {
                return user.Email;
            }
            else
            {
                return null;
            }
        }

        public bool IsEmailVerified(string id)
        {
            var user = DB.Users.Where(u => u.Id == id).FirstOrDefault();
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        //----------------------------------------------------------------------------------------------------
        //---------------------------------------User Chat Options--------------------------------------------
        public IEnumerable<Chat> GetUsersChats(string applicantId, string recipientId)
        {
            ApplicationUser userApplicant = Get(applicantId);
            ApplicationUser userRecipient = Get(recipientId);
            var listChat = DB.Chats.ToList();

            if (userApplicant != null && userRecipient != null)
            {
                if (listChat.Any(l => l.UserId == userApplicant.Id && l.ResponderId == userRecipient.Id))
                {
                    var applicantMessages = listChat.ToList().Where(c => c.UserId == userApplicant.Id && c.ResponderId == userRecipient.Id);
                    return applicantMessages;
                }
                return null;
            }
            return null;
        }

        public async Task MarkMessagesAsReadAsync(IEnumerable<Chat> messages)
        {
            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.IsRead = true;
                DB.Chats.AddOrUpdate(message);
            }

            await DB.SaveChangesAsync();
        }
    }
}