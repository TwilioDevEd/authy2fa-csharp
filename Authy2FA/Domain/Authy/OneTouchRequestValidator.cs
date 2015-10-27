using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebGrease.Css.Extensions;

// ReSharper disable PossibleNullReferenceException

namespace Authy2FA.Domain.Authy
{
    public class OneTouchRequestValidator
    {
        private readonly string _apiKey;
        private readonly HttpRequestBase _request;

        public OneTouchRequestValidator(string apiKey, HttpRequestBase request)
        {
            _apiKey = apiKey;
            _request = request;
        }

        public bool Validate()
        {
            var nonce = _request.Headers["X-Authy-Signature-Nonce"];
            var url = string.Format("{0}://{1}{2}",
                _request.Url.Scheme, _request.Headers["X-Original-Host"], _request.Url.AbsolutePath);
            var serialized = Serialize(Sort(Parameters)).Trim('&');

            var data = string.Format("{0}|{1}|{2}|{3}",
                nonce, _request.HttpMethod, url, serialized);

            var digest = ComputeDigest(data, _apiKey);
            var authySignature = _request.Headers["X-Authy-Signature"];

            return digest == authySignature;
        }

        private JObject Parameters
        {
            get
            {
                _request.InputStream.Position = 0;
                return (JObject) JsonConvert.DeserializeObject(new StreamReader(_request.InputStream).ReadToEnd());
            }
        }

        private static JObject Sort(JObject content)
        {
            var result = new JObject();

            var properties = content.Properties().OrderBy(property => property.Name);
            properties.ForEach(property =>
            {
                var propertyValue = property.Value as JObject;
                if (propertyValue != null)
                {
                    result.Add(property.Name, Sort(propertyValue));
                }
                else
                {
                    result.Add(property);
                }
            });

            return result;
        }

        private static string Serialize(JObject content)
        {
            var result = new StringBuilder();
            var properties = content.Properties();
            properties.ForEach(property =>
            {
                var propertyValue = property.Value as JObject;
                if (propertyValue != null)
                {
                    result.Append(Serialize(propertyValue));
                }
                else
                {
                    result.Append(string.Format("{0}={1}&",
                        FormatPath(property.Path), Encode(property.Value.ToString())));
                }
            });

            return result.ToString();
        }

        private static string FormatPath(string property)
        {
            var pathComponents = property.Split('.');
            var head = pathComponents[0];
            if (pathComponents.Length == 1)
            {
                return head;
            }

            var tail = pathComponents
                .Skip(1)
                .Select(component => string.Format("%5B{0}%5D", component));

            return string.Format("{0}{1}", head, string.Join("", tail));
        }

        private static string ComputeDigest(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            using (var hmacsha256 = new HMACSHA256(encoding.GetBytes(secret)))
            {
                var hashedMessage = hmacsha256.ComputeHash(encoding.GetBytes(message));
                return Convert.ToBase64String(hashedMessage);
            }
        }

        private static string Encode(string content)
        {
            return content
                .Replace("@", "%40")
                .Replace("=", "%3D")
                .Replace("/", "%2F")
                .Replace("+", "%2B")
                .Replace(" ", "+")
                .Replace("False", "false");
        }
    }
}