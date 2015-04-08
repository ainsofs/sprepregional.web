using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SPREPREGIONAL.Web {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}
