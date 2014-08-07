using System;

namespace MvcSample.Web.Us
{
    [Country("US")]
    public class CheckoutController
    {
        public object Index()
        {
            return "This is the US only controller.";
        }
    }
}