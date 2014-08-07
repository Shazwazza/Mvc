using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Framework.Logging;

namespace MvcSample.Web
{
    public class CountryRoute : TemplateRoute
    {
        public CountryRoute(IRouter target, string routeName, string routeTemplate, 
            IDictionary<string, object> defaults, IDictionary<string, object> constraints, 
            IInlineConstraintResolver inlineConstraintResolver) : 
            base(target, routeName, routeTemplate, defaults, constraints,  inlineConstraintResolver)
        {
        }

        public override Task RouteAsync(RouteContext context)
        {
            var host = context.HttpContext.Request.Host;

            var split = host.Value.Split(',');

            string countryCode = null;
            if (split.Length > 3)
            {
                countryCode = split[0];
            }

            context.RouteData.Values["Country"] = split;
            return base.RouteAsync(context);
        }
    }
}