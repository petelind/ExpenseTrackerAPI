using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Routing;

namespace ExpenseTracker.API.Helpers
{
    public class VersionConstraint :IHttpRouteConstraint
    {

        public const string VersionHeaderName = "api-version";
        private const int DefaultVersion = 1;

        public VersionConstraint(int allowedVersion)
        {
            AllowedVersion = allowedVersion;
        }

        public int AllowedVersion { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values,
            HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                // first lets look for the header "api-version"
                int? version = GetVersionFromCustomRequestHeader(request);

                // if failed - lets look into custom content type
                if (version == null) version = GetVersionFromCustomContentType(request);

                // reads like : "if version is null - then make DefaultVersion the version, 
                // and then return return result of comparing version to AllowedVersion
                return ((version ?? DefaultVersion) == AllowedVersion);
            }

            return true;
        }

        private int? GetVersionFromCustomRequestHeader(HttpRequestMessage request)
        {
            string versionAsString = null;
            IEnumerable<string> headerValues;

            // if we got a custom header, its "api-version" and it contains version...
            if (request.Headers.TryGetValues(VersionHeaderName, out headerValues) && headerValues.Count()==1)
            {
                versionAsString = headerValues.First(); // get it
            }
            else
            {
                return null; // else we were not able to identify version from message passed to us
            }

            int version; // if we got version as string - lets make it int
            if (versionAsString != null && Int32.TryParse(versionAsString, out version)) return version;

            // as safety precaution - if we failed on any step we assume we were not able to identify version
            return null;

        }

        private int? GetVersionFromCustomContentType(HttpRequestMessage request)
        {
            string versionAsString = null;
            Regex requiredMediaType = new Regex(@"application\/vnd\.expensetrackerapi\.v([\d]+)\+json"); // any MediaType matching this pattern is what we are looking for
            string matchingMediaType = null; // MediaType matching criteria will be here

            // collect all header which are about media types
            var mediaTypes = request.Headers.Accept.Select(h => h.MediaType);

            // look into them one by one...
            foreach (var mediaType in mediaTypes)
            {
                if (requiredMediaType.IsMatch(mediaType)) // searching for the match
                {
                    matchingMediaType = mediaType;
                    break; // we found it - no need to look further
                }
            }

            if (matchingMediaType == null) // we didnt found anything, we cannot identify API version
            {
                return null;
            }

            // there is a match, lets extract version out of it
            Match match = requiredMediaType.Match(matchingMediaType);
            versionAsString = match.Groups[1].Value;

            // is version convertable to int? If yes - lets do it and ship back
            int version;
            if ((versionAsString != null) && (Int32.TryParse(versionAsString, out version))) return version;

            // if we were not able to process conversion - we dont know version, so we confirm this fact
            return null;


        }

    }
}