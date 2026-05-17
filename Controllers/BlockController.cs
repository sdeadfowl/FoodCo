using FoodCo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodCo.Controllers
{
    public class BlockController : Controller
    {
        private ApplicationDbContext DB = new ApplicationDbContext();
        private readonly UserRepository _userRepository;

        public BlockController()
        {
            _userRepository = new UserRepository(new ApplicationDbContext());
        }

        [HttpPost]
        public JsonResult Blocking(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser blockedUser = _userRepository.Get(id);
            Block block = new Block()
            {
                BlockerId = currentUser.Id,
                BlockedId = blockedUser.Id,
                BlockDate = DateTime.Now,
                BlockStatus = EnumBlockStatus.Blocked,
            };
            DB.Blocks.AddOrUpdate(block);
            DB.SaveChanges();
            return Json(block, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UnBlock(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            ApplicationUser unBlockedUser = _userRepository.Get(id);
            Block block = new Block()
            {
                BlockerId = currentUser.Id,
                BlockedId = unBlockedUser.Id,
                BlockDate = DateTime.Now,
                BlockStatus = EnumBlockStatus.Unblocked,
            };
            DB.Blocks.AddOrUpdate(block);
            DB.SaveChanges();
            return Json(block, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Private(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            currentUser.IsPrivate = true;
            DB.Users.AddOrUpdate(currentUser);
            DB.SaveChanges();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Public(string id)
        {
            ApplicationUser currentUser = OnLineUsers.GetSessionUser();
            currentUser.IsPrivate = false;
            DB.Users.AddOrUpdate(currentUser);
            DB.SaveChanges();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}