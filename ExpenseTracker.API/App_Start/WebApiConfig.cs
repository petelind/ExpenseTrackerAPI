using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ExpenseTracker.API
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = new HttpConfiguration();

            // First, lets clear ATOM \ XML and other sh*t from supported formats
            config.MapHttpAttributeRoutes();
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            // Then lets create default mapping for ApiControllers inside API "folder"
            config.Routes.MapHttpRoute(name: "DefaultRouting",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }); // we sending in nameless object (lecture 4 of previous course)

            // Lets make JSON nicely indented...
            config.Formatters.JsonFormatter.SerializerSettings.Formatting
                = Newtonsoft.Json.Formatting.Indented;
            
            // ...and looking JS-style
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver
                = new CamelCasePropertyNamesContractResolver();

            // This will enable new content type we use for our PATCH method
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json-patch+json"));

            // This will configure cache server we use (simple memory-caching, by default)
            config.MessageHandlers.Add(new CacheCow.Server.CachingHandler(config));

            return config;
             
        }
    }
}
