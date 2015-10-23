using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Authy.Net;
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
            var user = await UserManager.FindByAuthyIdAsync(authy_id);
            user.AuthyStatus = status;
            await UserManager.UpdateAsync(user);

            return Content("Ok");
        }

        //
        // GET: Authy/OneTouchStatus
        public ActionResult OneTouchStatus()
        {
            // TODO: Figure out when the user approved the OneTouch request.
            // TODO: Validate with the API what are the values returned.
            // status = user.AuthyStatus
            return Content("status");
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
    }
}