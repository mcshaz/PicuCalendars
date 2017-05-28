using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static PicuCalendars.Security.ValidationUtilities;

namespace PicuCalendars.Security
{
    public class AccessUrlHandler : AuthorizationHandler<AccessRoute>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public AccessUrlHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessRoute requirement)
        {
            string claimType = requirement.AccessLevel.ToString();
            var rosterIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == claimType);
            if (rosterIdClaim == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var claimGuid = Guid.Parse(rosterIdClaim.Value);
            var routeGuid = Guid.Parse((string)_contextAccessor.HttpContext.GetRouteData().Values[requirement.RouteComponent]);
            if (routeGuid == claimGuid)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}
