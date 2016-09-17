using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ScheduleBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            DocumentDbRepository<Item>.Initialize();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
