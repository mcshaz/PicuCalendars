using System;
using System.Security.Cryptography;
using System.Text;

namespace PicuCalendars.Security
{
    public static class ValidationUtilities
    {
        public static string GetCreateSecret()
        {
                //todo move from hardcoded
            return "hJ4blpw8qTI5PEFpdzM2cNpnf6qTNzSBys+x8zQjjojcrVLvPPrXbmpg1ICXgYAg7AprMxz5EiOId9qOR8avzw==";
        }
        public static ValidationResult Validate(string timeStamp, Guid resourceId, byte[] secret, string token)
        {

            var dif = DateTime.Now - DateTime.Parse(timeStamp);
            if (dif.TotalSeconds > 45.0 || dif.TotalSeconds < -15.0)
            {
                return ValidationResult.TokenExpired;
            }

            if (token == Hash(timeStamp, resourceId, secret))
            {
                return ValidationResult.Valid;
            }
            return ValidationResult.InvalidToken;
        }

        public static string Hash(string timeStamp, Guid resourceId, string base64Secret)
        {
            return Hash(timeStamp, resourceId, Convert.FromBase64String(base64Secret));
        }

        public static string Hash(string timeStamp, Guid resourceId, byte[] secret)
        {
            var salted = Encoding.UTF8.GetBytes(timeStamp + resourceId.ToString());
            using (var crypto = new HMACSHA256(secret))
            {
                crypto.ComputeHash(salted);
                return Convert.ToBase64String(crypto.Hash);
            }
        }

        public enum ValidationResult { Valid, TokenExpired, InvalidToken }


        public class RequestClaim
        {
            public enum AccessLevel { CreateRoster, SpecificRoster }
            public AccessLevel Access { get; set; }
            public Guid ResourceId { get; set; }
            public string Token { get; set; }
        }
    }
}
