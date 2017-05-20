using Newtonsoft.Json;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using PicuCalendars.Security;
using System;
using System.Collections;
using System.Collections.Generic;
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
        const string _domain = "http://localhost:51558/api";
#else
        const string _domain = "http://rosters.sim-planner.com";
#endif
        const string _dateHeaderFormat = "ddd, dd MMM yyyy hh:mm:ss";
        const string _jsonContentType = "application/json";

        internal static HttpResponseMessage CreateRoster(Roster data)
        {
            var now = DateTime.Now.ToString(_dateHeaderFormat);

            var cookieContainer = new CookieContainer();

            var baseAddress = new Uri(_domain);
            using (var aes = new SimpleAes())
            {
                var claim = new RequestClaim
                {
                    Access = RequestClaim.AccessLevel.CreateRoster,
                    ResourceId = data.Id,
                    Token = Hash(now, data.Id, GetCreateSecret())
                };

                var jsonClaim = JsonConvert.SerializeObject(claim);
                var encryptedString = aes.Encrypt(jsonClaim);
                cookieContainer.Add(baseAddress, new Cookie("token", encryptedString));
            }

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient { BaseAddress = baseAddress })
                {
                    client.DefaultRequestHeaders.Add("Date", now);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, _jsonContentType);
                    return client.PostAsync("roster", content).Result;
                }
            }
        }
        public static HttpResponseMessage PostRosterUpsert(Guid rosterId, string base64Secret,object data)
        {

            //object first = data.FirstOrDefault();

            //if (first == null) { return; }

            var now = DateTime.Now.ToString(_dateHeaderFormat);
            var cookieContainer = new CookieContainer();


            var baseAddress = new Uri(_domain);
            using (var aes = new SimpleAes())
            {
                var claim = new RequestClaim {
                    Access = RequestClaim.AccessLevel.SpecificRoster,
                    ResourceId = rosterId,
                    Token = Hash(now, rosterId, base64Secret)
                };

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
                using (var client = new HttpClient { BaseAddress =  baseAddress})
                {
                    client.DefaultRequestHeaders.Add("Date", now);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, _jsonContentType);
                    return client.PostAsync(type.Name + '/' +rosterId.ToString(), content).Result;
                }
            }
        }
    }
}
