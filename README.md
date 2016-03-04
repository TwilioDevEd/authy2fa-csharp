# Two-Factor Authentication with Authy in ASP.NET MVC

This example application demonstrates how to use [Authy](http://www.authy.com) as the two-factor authentication provider in an ASP.NET MVC project using [ASP.NET Identity](http://www.asp.net/identity/overview/getting-started/introduction-to-aspnet-identity).

For more information on how this code works, [check out our interactive code walkthrough](http://www.twilio.com/docs/howto/walkthrough/two-factor-authentication/csharp/mvc#1).

## Local Development

1. First clone this repository and `cd` into its directory:
   ```
   git clone git@github.com:TwilioDevEd/authy2fa-csharp.git

   cd authy2fa-csharp
   ```

2. Create a new file `Authy2FA/Local.config` and update the content with:
   ```
   <appSettings>
     <add key="webpages:Version" value="3.0.0.0" />
     <add key="webpages:Enabled" value="false" />
     <add key="ClientValidationEnabled" value="true" />
     <add key="UnobtrusiveJavaScriptEnabled" value="true" />
     <add key="AuthyKey" value="your authy production key" />
   </appSettings>
   ```

3. Build the solution.

4. Run `Update-Database` at [Package Manager
   Console](https://docs.nuget.org/consume/package-manager-console) to execute the migrations.

5. Run the application.

6. Check it out at http://localhost:49217

That's it!

To let Authy OneTouch to use the callback endpoint you exposed, your development server will need to be publicly accessible. [We recommend using ngrok to solve this problem](https://www.twilio.com/blog/2015/09/6-awesome-reasons-to-use-ngrok-when-testing-webhooks.html).

## Meta

* No warranty expressed or implied. Software is as is. Diggity.
* [MIT License](http://www.opensource.org/licenses/mit-license.html)
* Lovingly crafted by Twilio Developer Education.

---------------
<a href="http://twilio.com/signal">![](https://s3.amazonaws.com/baugues/signal-logo.png)</a>

Join us in San Francisco May 24-25th to [learn directly from the developers who build Authy](https://www.twilio.com/signal/schedule/2crLXWsVZaA2WIkaCUyYOc/aut). 

