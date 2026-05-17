using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodCo.Controllers
{
    public class SearchController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}
