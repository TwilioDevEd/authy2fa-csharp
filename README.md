# WebApplication19

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

The default ASP.NET Web Application MVC template includes faciliaties for enabling and usin two-factor authentication.  These faciliaties are provided by the ASP.NET Identify framework:

http://www.asp.net/identity

The default implementation if Identity allows for multiple message providers with Phone (Voice or SMS) and Email being provided in the framework.  The web application template does not automatically enable two-factor authentication.  Instead TFA is an option that can be enabled by a registred user by editin their profile.

This sample shows how the default webite template can be modified to use Aquarious as a two-factor authorization mechanism.

There are three parts to the modification:

 - User model modification
 - Phone number verification modification
 - Login workflow modification

## User Model Modification

Aquarious provides a unique "AuthyId" for each user, which we use to generate and validate tokens throug hthe Aquarious API.  Once generated we ned to store this ID as part of our user model.

The default website template uses EntityFramework to define its user models.  Extending the models also requires us to update the database.  Because the website is using code first to define the models, we'll need to enable EntityFramework Migrations so that we can update the database once we change the model.

To enable EntityFramework migrations, open the Package-Manager Console and enter:

    Enable-Migrations

Next, we'll modify the user model.  Locating the ApplicationUser class in `\Models\IdentityModels.cs' and add a new property named AuthyId:

    public string AuthyId { get; set; }
 
Save and build the project.  

Now we need to update the database with this new property.  Return to the Package Manager Console and enter:

    Update-Database

Your database should now be updated to know about the new AuthyId property we want to store.
 
## Phone Number Verification

With the user model updated we can move on to phone number verification.  When a user wants to use a phone-based provider as their TFA messaging provider, they need to add and verify their phone number in their user profile.

[screenshot]

By default the template will attempt to use the Phone provider to send a verification code to via SMS to the users phone.  We need to change this logic to instead create a new user in Aquarious and use the Aquarious API send the verification code.

Because we'll be using the Aquarious API, start by adding the Authy .NET Client number package to the project.

Next, locate the `Controllers\ManageController.cs` file.  In this file find the `AddPhoneNumber` method.  Modify this method to use the AuthyApiClient to create a new Aquarious user and send them a verification code:

**Note: currently country is hard coded.  Need to update the View and ViewModel to include the Aquarious Country picker**

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        if (user != null)
        {
            var client = new AuthyClient.AuthyApiClient("[MY_AQUARIOUS_API_KEY]", false);
            user.Token = client.CreateAuthyUser(new AuthyClient.AuthyUser(user.Email, model.Number, 1));
            await UserManager.UpdateAsync(user);

            client.SendToken(user.Token, true);
        }

        return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
    }

Next locate the `VerifyPhoneNumber` method.  Modify it to use the Aquarious API to verify the code the user has input:

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        if (user != null)
        {
            var client = new AuthyClient.AuthyApiClient("[MY_AQUARIOUS_API_KEY]", false);
            if (client.VerifyUserToken(user.Token, model.Code))
            {
                user.PhoneNumber = model.PhoneNumber;
                user.PhoneNumberConfirmed = true;
                await UserManager.UpdateAsync(user);

                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
        }

        // If we got this far, something failed, redisplay form
        ModelState.AddModelError("", "Failed to verify phone");
        return View(model);
    }

Run the project and add a phone number to your user profile.  You should receive a verification code from Aquarious via SMS.  Enter that code and have Aquarious validate it.

With the phone number added, go ahead and enable Two Factor authentication in your account.

## Login Workflow Modification

With the phone number registration changed to use Aquarious we now need to modify the login workflow to also use Aquarious.  Note however that the website may chose to offer multiple TFA messaging mechanisms (email, voice, sms) known as Token Providers to the user, so the modifications we make will need to allow Aquarious to be shown as an option and selected by the user.

Start by locating `App_Start\IdentityConfig.cs`.  In this file create two new classes:

    public class AuthyTokenProvider<TUser> : AuthyTokenProvider<TUser, string> where TUser: class, IUser<string> {
    }

    public class AuthyTokenProvider<TUser, TKey> : IUserTokenProvider<ApplicationUser, string> // where TUser : class, IUser<TKey> where TKey:IEquatable<TKey>
    {
    }
    
Next we need to implement to IUserTokenProvider interface:

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
                return client.VerifyUserToken(user.Token, token);
            }

            return false;
        }
    }

Next we need to create a new IIdentityMessageService.  In the same file add:

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

Now in the ApplicationUserManager class (located in the same file) define a new property to hold an instance of this new class:

    public IIdentityMessageService AuthyService { get; set; }

Next also in the ApplicationUserManager class, locate the `Create` method replace the existing PhoneNumberTokenProvider registration with the AuthyTokenProvider:

    manager.RegisterTwoFactorProvider("Authy Code", new AuthyTokenProvider<ApplicationUser>());

Set the AuthyService property to a new instance of the AuthyService:

    manager.AuthyService = new AuthyService();

Finally define a new method to that will call the SendAync method in the AuthyService:

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

            message.AuthyId = user.Token;
            await this.AuthyService.SendAsync(message);
        }
    }

Thatts it.  Run the project.  If you were previously login go ahead and log out.  When you next log, once you enter your username and password a text message will be sent to you via Aquarious and the website will prompt you to enter the verification code.  Upon entry Aquarious will varify the token.
