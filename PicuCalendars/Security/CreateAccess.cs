using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PicuCalendars.Security.ValidationUtilities;

namespace PicuCalendars.Security
{
    public class AccessRoute : IAuthorizationRequirement
    {
        public AccessRoute(RequestClaimBase.AccessLevel accessLevel = RequestClaimBase.AccessLevel.UpdateResource, string routeComponent = "id") {
            _accessLevel = accessLevel;
            _routeComponent = routeComponent;

        }
        public RequestClaimBase.AccessLevel AccessLevel { get { return _accessLevel; } }
        public string RouteComponent { get { return _routeComponent; } }

        private readonly RequestClaimBase.AccessLevel _accessLevel;
        private readonly string _routeComponent;
    }
}
