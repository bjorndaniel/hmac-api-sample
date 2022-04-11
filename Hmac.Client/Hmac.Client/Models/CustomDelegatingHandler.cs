using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hmac.Client.Models
{
    public class CustomDelegatingHandler : HttpClientHandler
    {
        private const string _apiKey = "";
        private const string _appId = "";

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContentBase64String = string.Empty;
            var requestUri = System.Web.HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());
            var requestHttpMethod = request.Method.Method;

            //Calculate UNIX time
            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - epochStart;
            var requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            //create random nonce for each request
            var nonce = Guid.NewGuid().ToString("N");

            //Checking if the request contains body, usually will be null wiht HTTP GET and DELETE
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsByteArrayAsync();
                using var sha256 = SHA256.Create();
                //Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
                var requestContentHash = sha256.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            //Creating the raw signature string
            var signatureRawData = $"{_appId}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";
            var secretKeyByteArray = Convert.FromBase64String(_apiKey);
            var signature = Encoding.UTF8.GetBytes(signatureRawData);

            using var hmac = new HMACSHA256(secretKeyByteArray);
            var signatureBytes = hmac.ComputeHash(signature);
            var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
            //Setting the values in the Authorization header using custom scheme (amx)
            request.Headers.Authorization = new AuthenticationHeaderValue("amx", $"{_appId}:{requestSignatureBase64String}:{nonce}:{requestTimeStamp}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}