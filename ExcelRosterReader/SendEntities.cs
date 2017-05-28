using Newtonsoft.Json;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using PicuCalendars.Security;
using PicuCalendars.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                Access = RequestClaimBase.AccessLevel.CreateResource,
                ResourceId = data.Id
            };
            return Post(data, claim, GetCreateSecret(), message, error);
        }
        public static HttpResponseMessage PostRosterUpsert(Guid rosterId, string base64Secret,object data, TextWriter message, TextWriter error)
        {
            var claim = new RequestClaim
            {
                Access = RequestClaimBase.AccessLevel.UpdateResource,
                ResourceId = rosterId
            };
            return Post(data, claim, base64Secret, message, error);
        }

        private static HttpResponseMessage Post(object data, RequestClaim claim, string base64Secret, TextWriter message, TextWriter error)
        {
            var type = data.GetType();
            var ie = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ie == null)
            {
                if (!ValidateAttributes.IsValid(data, error))
                {
                    return null;
                }
            }
            else
            {
                type = ie.GetGenericArguments()[0];
                foreach (object o in (IEnumerable)data)
                {
                    if (!ValidateAttributes.IsValid(o, error))
                    {
                        return null;
                    }
                }
            }

            var baseAddress = new Uri(_domain);

            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    client.DefaultRequestHeaders.Date = new DateTimeOffset(DateTime.Now);
                    using (var aes = new SimpleAes())
                    {
                        claim.Token = Hash(client.DefaultRequestHeaders.GetValues("Date").First(), claim.ResourceId, base64Secret);

                        var jsonClaim = JsonConvert.SerializeObject(claim);
                        var encryptedString = aes.Encrypt(jsonClaim);
                        var cookieContainer = new CookieContainer();
                        cookieContainer.Add(baseAddress, new Cookie("token", encryptedString));
                        handler.CookieContainer = cookieContainer;
                    }
                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("text/html"));
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, _jsonContentType);
                    string requestUri = $"api/{type.Name}/{claim.ResourceId}";
                    message.WriteLine($"Posting to {baseAddress.AbsoluteUri}{requestUri}");
                    HttpResponseMessage response;
                    try
                    {
                        //response = client.GetAsync(requestUri).Result;
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

#if DEBUG
                    bool debug = true;
#else
                    bool debug = false;
#endif

                    if (!response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0)
                    {
                        var tempFile = Path.GetTempFileName() + ".html";
                        using (var fileStream = File.Create(tempFile))
                        {
                            using (var responseStream = response.Content.ReadAsStreamAsync().Result)
                            {
                                responseStream.CopyTo(fileStream);
                            }
                        }
                        System.Diagnostics.Process.Start(tempFile);
                    }
                    else if (debug)
                    {
                        var msg = response.Content.ReadAsStringAsync().Result;
                        if (msg != string.Empty)
                        {
                            m.WriteLine(msg);
                        }
                    }

                    return response;
                }
            }
        }
    }
}
