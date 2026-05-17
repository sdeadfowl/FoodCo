using System.Web.Mvc;

namespace FoodCo.Controllers
{
    public class ErrorController : Controller
    {
        // Error 404
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return PartialView("~/Views/Shared/ErrorPartials/_PageNotFound.cshtml");
        }

        // Error 500
        public ActionResult InternalError()
        {
            Response.StatusCode = 500;
            return PartialView("~/Views/Shared/ErrorPartials/_InternalError.cshtml");
        }

        // Error 403
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/ErrorPartials/_Forbidden.cshtml");
        }

        // Error 401
        public ActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            return PartialView("~/Views/Shared/ErrorPartials/_Unauthorized.cshtml");
        }

        // Error 400
        public ActionResult BadRequest()
        {
            Response.StatusCode = 400;
            return PartialView("~/Views/Shared/ErrorPartials/_BadRequest.cshtml");
        }

        // General Error (for any other errors)
        public ActionResult GeneralError()
        {
            return PartialView("~/Views/Shared/ErrorPartials/_GeneralError.cshtml");
        }
    }
}
