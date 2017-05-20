using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PicuCalendars.DataAccess;
using PicuCalendars.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PicuCalendars.Security.ValidationUtilities;

namespace PicuCalendars.Security
{
    internal static class MyValidationToken
    {
        public static async Task<Guid?> RosterAccess(HttpContext httpCon, CalendarContext calCon)
        {
            string token = httpCon.Request.Cookies["token"];
            if (token==null)
            {
                return null;
            }
            string timeStamp = httpCon.Request.Headers["Date"];
            if (timeStamp == null)
            {
                return null;
            }
            string decryptedString;
            using (var aes = new SimpleAes())
            {
                decryptedString = aes.Decrypt(token);
            }
            var claim = JsonConvert.DeserializeObject<RequestClaim>(decryptedString);

            byte[] secret;
            switch (claim.Access)
            {
                case RequestClaim.AccessLevel.CreateRoster:
                    secret = Convert.FromBase64String(GetCreateSecret());
                    break;
                case RequestClaim.AccessLevel.SpecificRoster:
                    secret = (await calCon.Rosters.FindAsync(claim.ResourceId)).Secret;
                    break;
                default:
                    throw new Exception("Enum not found - " + claim.Access.ToString());
            }

            var result = ValidationUtilities.Validate(timeStamp, claim.ResourceId, secret, claim.Token);

            //return result == ValidationResult.Valid;

            if (result == ValidationResult.Valid)
            {
                //todo change to not authorized exception
                switch (claim.Access)
                {
                    case RequestClaim.AccessLevel.CreateRoster:
                    case RequestClaim.AccessLevel.SpecificRoster:
                        return claim.ResourceId;
                    default:
                        throw new Exception("Enum not found - " + claim.Access.ToString());
                }
            }
            //will be handled by auth attribute
            //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            httpCon.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(result.ToString().ToSeperateWords()));
            return null;
        }
    }
}
