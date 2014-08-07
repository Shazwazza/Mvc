using System;
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.GeneralCountry
{
    [CatchAllCountry]
    public class CheckoutController : Controller
    {
        public object Index()
        {
            return "This is the general checkout for country - " + ActionContext.RouteData.Values["CountryCode"];
        }
    }
}