using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Authy.Net;
using Newtonsoft.Json;

namespace Authy2FA.Domain.Authy
{
    public class OneTouch
    {
        private readonly string _apiKey;
        private readonly string _userId;

        /// <summary>
        /// Creates a new instance of the OneTouch client
        /// </summary>
        /// <param name="apiKey">The api key used to access the rest api</param>
        /// <param name="userId">The user id used to send approval request</param>
        public OneTouch(string apiKey, string userId)
        {
            _apiKey = apiKey;
            _userId = userId;
        }

        /// <summary>
        /// Send Approval Request to Authy
        /// </summary>
        /// <param name="message">Message to display in the device</param>
        /// <param name="email">Email address</param>
        /// <returns></returns>
        public SendApprovalRequestResult SendApprovalRequest(string message, string email)
        {
            var request = new NameValueCollection
            {
                {"details[email]", email}
            };

            var url = string.Format("{0}/onetouch/json/users/{1}/approval_requests?api_key={2}&message={3}",
                BaseUrl, _userId, _apiKey, message);

            return Execute(client =>
            {
                var response = client.UploadValues(url, request);
                var textResponse = Encoding.ASCII.GetString(response);

                var apiResponse = JsonConvert.DeserializeObject<SendApprovalRequestResult>(textResponse);
                apiResponse.RawResponse = textResponse;
                apiResponse.Status = AuthyStatus.Success;

                return apiResponse;
            });
        }

        private static TResult Execute<TResult>(Func<WebClient, TResult> execute)
            where TResult : AuthyResult
        {
            var client = new WebClient();

            try
            {
                return execute(client);
            }
            catch (WebException webex)
            {
                var response = webex.Response.GetResponseStream();

                string body;
                using (var reader = new StreamReader(response))
                {
                    body = reader.ReadToEnd();
                }

                var result = JsonConvert.DeserializeObject<TResult>(body);
                return result;
            }
            finally
            {
                client.Dispose();
            }
        }

        private static string BaseUrl
        {
            get { return "https://api.authy.com"; }
        }
    }
}