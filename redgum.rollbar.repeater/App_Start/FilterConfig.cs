using Rollbar;
using System.Web;
using System.Web.Mvc;

namespace redgum.rollbar.repeater
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RollbarExceptionFilter());
        }
    }

    public class RollbarExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            RollbarLocator.RollbarInstance.Error(filterContext.Exception);
        }
    }
}
