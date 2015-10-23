using System;
using System.Configuration;
using System.Threading.Tasks;
using Authy.Net;
using Authy2FA.Domain.Authy;
using Microsoft.AspNet.Identity;

namespace Authy2FA.Providers
{
    public class AuthyOneTouchProvider<TUser> : AuthyOneTouchProvider<TUser, string> where TUser : class, IUser<string>
    {
        public AuthyOneTouchProvider(string authyIdPropertyName) : base(authyIdPropertyName) { }
    }

    public class AuthyOneTouchProvider<TUser, TKey> : IUserTokenProvider<TUser, TKey>
        where TUser : class, IUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly string _authyKey = ConfigurationManager.AppSettings["AuthyKey"];

        public AuthyOneTouchProvider(string authyIdPropertyName)
        {
            this.AuthyIdPropertyName = authyIdPropertyName;
        }

        public string AuthyIdPropertyName { get; private set; }

        public Task<string> GenerateAsync(string purpose, UserManager<TUser, TKey> manager, TUser user)
        {
            //this can just return null becuase we're not actually generating the code
            return Task.FromResult((string)null);
        }

        public async Task<bool> IsValidProviderForUserAsync(UserManager<TUser, TKey> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            var phoneNumber = await manager.GetPhoneNumberAsync(user.Id);
            return !string.IsNullOrWhiteSpace(phoneNumber);
        }

        public Task NotifyAsync(string token, UserManager<TUser, TKey> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            var authyId = FindAuthyId(user);
            var email = FindEmail(user);
            var oneTouchClient = new OneTouchClient(_authyKey, authyId);
            oneTouchClient.SendApprovalRequest("Request login to Twilio demo app",
                email);

            return Task.FromResult(0);
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser, TKey> manager, TUser user)
        {
            if (user == null) return false;
            var client = new AuthyClient(_authyKey);
            var result = client.VerifyToken(FindAuthyId(user), token, true);

            return result.Status == AuthyStatus.Success;
        }

        private string FindAuthyId(TUser user)
        {
            var userType = user.GetType();
            var authyIdProp = userType.GetProperty(this.AuthyIdPropertyName);

            if (authyIdProp == null)
                throw new NotImplementedException("A property named {0} could not be found on the user model");

            var id = authyIdProp.GetValue(user);

            return (string)id;
        }

        private static string FindEmail(TUser user)
        {
            var userType = user.GetType();
            var emailProp = userType.GetProperty("Email");

            if (emailProp == null)
                throw new NotImplementedException("A property named {0} could not be found on the user model");

            var email = emailProp.GetValue(user);

            return (string)email;
        }
    }
}