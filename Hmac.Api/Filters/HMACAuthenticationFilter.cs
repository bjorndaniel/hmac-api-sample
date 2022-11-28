using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Hmac.Api.Filters
{
    public class HMACAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        private const string AuthorizationHeader = "Authorization";

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
            if (hasAllowAnonymous)
            {
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(AuthorizationHeader, out var extractedApiKey))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided"
                };
                return;
            }

            var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>("ApiKey");
            var appId = appSettings.GetValue<string>("AppId");

            if (!(await ValidateRequest(extractedApiKey.First(), apiKey, appId, context.HttpContext)))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    Content = "Api Key is not valid"
                };
                return;
            }
        }


        private async Task<bool> ValidateRequest(string extractedApiKey, string apiKey, string appId, HttpContext context)
        {
            var valueString = extractedApiKey.Split(" ")[1];
            var values = valueString.Split(":").ToList();
            if (values.Count() == 4)
            {
                var requestUri = HttpUtility.UrlEncode(context.Request.GetEncodedUrl());
                context.Request.EnableBuffering();
                
                var bytes = new byte[context.Request.ContentLength ?? 0];
                await context.Request.Body.ReadAsync(bytes);
                var suppliedAppId = values[0];
                var requestString = values[1];
                var nonce = values[2];
                var timeStamp = values[3];

                if (suppliedAppId != appId)
                {
                    return false;
                }
                if (IsReplayRequest(nonce, timeStamp, context))
                {
                    return false;
                }
                using var sha256 = SHA256.Create();
                //Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
                var requestContentHash = sha256.ComputeHash(bytes);
                var requestContentBase64String = bytes.Length == 0 ? string.Empty : Convert.ToBase64String(requestContentHash);

                var signatureRawData = $"{appId}{context.Request.Method}{requestUri}{timeStamp}{nonce}{requestContentBase64String}";
                var secretKeyByteArray = Convert.FromBase64String(apiKey);
                var signature = Encoding.UTF8.GetBytes(signatureRawData);

                using var hmac = new HMACSHA256(secretKeyByteArray);
                var signatureBytes = hmac.ComputeHash(signature);
                var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                context.Request.Body = new MemoryStream(bytes);
                return requestSignatureBase64String == requestString;
            }
            return false;
        }

        private bool IsReplayRequest(string nonce, string requestTimeStamp, HttpContext context)
        {
            var memoryCache = context.RequestServices.GetService<IMemoryCache>();
            if (memoryCache!.TryGetValue(nonce, out var no))
            {
                return false;
            }
            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var currentTs = DateTime.UtcNow - epochStart;

            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if ((serverTotalSeconds - requestTotalSeconds) > 300)
            {
                return true;
            }
            memoryCache!.Set(nonce, requestTimeStamp);
            return false;
        }
    }
}