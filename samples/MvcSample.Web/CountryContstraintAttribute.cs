using System;
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web
{
    public class CatchAllCountryAttribute : RouteConstraintAttribute
    {
        public CatchAllCountryAttribute() : base("CountryCode")
        {
        }
    }

    public class CountryAttribute: RouteConstraintAttribute
    {
        public CountryAttribute(string country) : base("CountryCode", country, false)
        {
        }
    }
}