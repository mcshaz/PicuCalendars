using Newtonsoft.Json;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using PicuCalendars.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using static PicuCalendars.Security.ValidationUtilities;

namespace ExcelRosterReader
{
    public static class SendEntities
    {
#if DEBUG
        const string _domain = "http://localhost:51558";
#else
        const string _domain = "http://rosters.sim-planner.com";
#endif
        const string _dateHeaderFormat = "ddd, dd MMM yyyy hh:mm:ss";
        const string _jsonContentType = "application/json";

        internal static HttpResponseMessage CreateRoster(Roster data, TextWriter message, TextWriter error)
        {
            var claim = new RequestClaim
            {
                Access = RequestClaim.AccessLevel.CreateRoster,
                ResourceId = data.Id
            };
            return Post(data, claim, GetCreateSecret(), message, error);
        }
        public static HttpResponseMessage PostRosterUpsert(Guid rosterId, string base64Secret,object data, TextWriter message, TextWriter error)
        {
            var claim = new RequestClaim
            {
                Access = RequestClaim.AccessLevel.SpecificRoster,
                ResourceId = rosterId
            };
            return Post(data, claim, base64Secret, message, error);
        }

        private static HttpResponseMessage Post(object data, RequestClaim claim, string base64Secret, TextWriter message, TextWriter error)
        {
            var now = DateTime.Now.ToString(_dateHeaderFormat);
            var cookieContainer = new CookieContainer();


            var baseAddress = new Uri(_domain);
            using (var aes = new SimpleAes())
            {
                claim.Token = Hash(now, claim.ResourceId, base64Secret);

                var jsonClaim = JsonConvert.SerializeObject(claim);
                var encryptedString = aes.Encrypt(jsonClaim);
                cookieContainer.Add(baseAddress, new Cookie("token", encryptedString));
            }

            var type = data.GetType();
            var ie = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ie != null)
            {
                type = ie.GetGenericArguments()[0];
            }
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    client.DefaultRequestHeaders.Add("Date", now);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, _jsonContentType);
                    string requestUri = $"api/{type.Name}/{claim.ResourceId}";
                    message.WriteLine($"Posting to {baseAddress.AbsoluteUri}{requestUri}");
                    HttpResponseMessage response;
                    try
                    {
                        response = client.PostAsync(requestUri, content).Result;
                    }
                    catch (Exception e)
                    {
                        while (e.InnerException != null) { e = e.InnerException; }
                        error.WriteLine(e.Message);
                        return null;
                    }
                    TextWriter m = response.IsSuccessStatusCode ? message : error;
                     m.WriteLine($"server returned {(int)response.StatusCode} ({response.ReasonPhrase})");
                    return response;
                }
            }
        }
    }
}
