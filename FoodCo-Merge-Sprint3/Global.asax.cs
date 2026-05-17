using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FoodCo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_Error()
        //{
        //    Exception exception = Server.GetLastError();
        //    Response.Clear();

        //    HttpException httpException = exception as HttpException;
        //    string action = "GeneralError"; // Default action for general errors

        //    if (httpException != null)
        //    {
        //        switch (httpException.GetHttpCode())
        //        {
        //            case 400:
        //                action = "BadRequest";
        //                break;
        //            case 401:
        //                action = "Unauthorized";
        //                break;
        //            case 403:
        //                action = "Forbidden";
        //                break;
        //            case 404:
        //                action = "PageNotFound";
        //                break;
        //            case 500:
        //                action = "InternalError";
        //                break;
        //            default:
        //                action = "GeneralError";
        //                break;
        //        }
        //    }

        //    // Log 
        //    // LogException(exception);

        //    Server.ClearError();
        //    Response.TrySkipIisCustomErrors = true; // Prevent IIS from showing its own error page

        //    Server.TransferRequest($"/Error/{action}");
        //}

    }
}
