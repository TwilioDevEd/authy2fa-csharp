using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Authy.Net;
using Authy2FA.Domain.Authy;
using Authy2FA.Models;
using Microsoft.AspNet.Identity.Owin;

namespace Authy2FA.Controllers
{
    public class AuthyController : Controller
    {
        private static readonly AuthyClient AuthyClient = new AuthyClient(ConfigurationManager.AppSettings["AuthyKey"]);

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        //
        // POST: Authy/Callback
        [HttpPost]
        // ReSharper disable once InconsistentNaming
        public async Task<ActionResult> Callback(string authy_id, string status)
        {
            if (!AuthenticateAuthyRequest())
            {
                throw new Exception("This request is unsigned");
            }

            var user = await UserManager.FindByAuthyIdAsync(authy_id);
            user.AuthyStatus = status;
            await UserManager.UpdateAsync(user);

            return Content("Ok");
        }

        //
        // POST: Authy/OneTouchStatus
        [HttpPost]
        public async Task<ActionResult> OneTouchStatus()
        {
            var user = await GetPartiallyLoggedUser();
            if (user == null)
            {
                throw new Exception("User is either not authenticated or already logged in");
            }

            var status = user.AuthyStatus;
            if (status == "denied")
            {
                user.AuthyStatus = string.Empty;
                await UserManager.UpdateAsync(user);

                return Content(status);
            }

            if (status == "approved")
            {
                var result = await SignInManager.TwoFactorSignInAsync("Authy One Touch", string.Empty, isPersistent: false, rememberBrowser: false);
                if (result == SignInStatus.Success)
                {
                    user.AuthyStatus = string.Empty;
                    await UserManager.UpdateAsync(user);
                }

                return Content(status);
            }

            return Content(status);
        }

        //
        // GET: Authy/SendToken
        public async Task<ActionResult> SendToken()
        {
            var user = await GetPartiallyLoggedUser();
            if (user == null) return Content("User is either not authenticated or already logged in");

            AuthyClient.SendSms(user.AuthyId);
            return Content("Token has been sent");
        }

        /// <summary>
        /// Get a partially logged user, the second step verification step is not yet done
        /// </summary>
        /// <returns>A user</returns>
        private async Task<ApplicationUser> GetPartiallyLoggedUser()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            return await UserManager.FindByIdAsync(userId);
        }

        private bool AuthenticateAuthyRequest()
        {
            var apiKey = ConfigurationManager.AppSettings["AuthyKey"];
            return new OneTouchRequestValidator(apiKey, Request).Validate();
        }
    }
}