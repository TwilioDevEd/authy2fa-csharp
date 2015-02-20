using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using WebApplication19.Models;
using AuthyClient;
using System.Globalization;

namespace WebApplication19
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class AuthyService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var m = (AuthyMessage)message;
            
            // Plug in your SMS service here to send a text message.            
            AuthyApiClient client = new AuthyApiClient("[MY_AQUARIOUS_API_KEY]", false);
            client.SendToken(m.AuthyId, true);

            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.

            manager.RegisterTwoFactorProvider("Authy Code", new AuthyTokenProvider<ApplicationUser>());
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });

            manager.EmailService = new EmailService();
            manager.AuthyService = new AuthyService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public virtual async Task SendAuthyAsync(string userId)
        {
            if (this.AuthyService != null)
            {
                var message = new AuthyMessage();
                var user = await FindByIdAsync(userId);
                if (user == null)
                {
                    //throw a user not found exception
                }

                message.AuthyId = user.AuthyId;
                await this.AuthyService.SendAsync(message);
            }
        }

        public IIdentityMessageService AuthyService { get; set; }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<bool> SendTwoFactorCodeAsync(string provider)
        {
            return base.SendTwoFactorCodeAsync(provider);
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class AuthyMessage : IdentityMessage
    {
        public virtual string AuthyId { get; set; }
    }

    public class AuthyTokenProvider<TUser> : AuthyTokenProvider<TUser, string> where TUser: class, IUser<string> {
    }

    public class AuthyTokenProvider<TUser, TKey> : IUserTokenProvider<ApplicationUser, string> // where TUser : class, IUser<TKey> where TKey:IEquatable<TKey>
    {
        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            string phoneNumber = await manager.GetPhoneNumberAsync(user.Id);
            return ("PhoneNumber:" + purpose + ":" + phoneNumber);
        }

        public async Task<bool> IsValidProviderForUserAsync(UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            string phoneNumber = await manager.GetPhoneNumberAsync(user.Id);
            return !string.IsNullOrWhiteSpace(phoneNumber);
        }

        public Task NotifyAsync(string token, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            return ((ApplicationUserManager)manager).SendAuthyAsync(user.Id);
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            if (user != null)
            {
                var client = new AuthyClient.AuthyApiClient("[MY_AQUARIOUS_API_KEY]", false);
                return client.VerifyUserToken(user.AuthyId, token);
            }

            return false;

        }
    }

}
