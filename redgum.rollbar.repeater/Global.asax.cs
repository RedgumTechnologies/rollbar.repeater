using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using redgum.rollbar.repeater.Services;
using Rollbar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace redgum.rollbar.repeater
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RollbarLocator.RollbarInstance.Configure(
                new Rollbar.RollbarConfig(AppSettingsProvider.GetSetting("Rollbar.AccessToken"))
                {
                    Environment = AppSettingsProvider.GetSetting("Rollbar.Environment"),
                    Enabled = bool.Parse(AppSettingsProvider.GetSetting("Rollbar.Enabled"))
                });

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
