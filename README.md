<a href="https://www.twilio.com">
  <img src="https://static0.twilio.com/marketing/bundles/marketing/img/logos/wordmark-red.svg" alt="Twilio" width="250" />
</a>

# Two-Factor Authentication with Authy

This application example demonstrates how to use [Authy](http://www.authy.com)
as the two-factor authentication provider in an ASP.NET MVC project using
[ASP.NET Identity](http://www.asp.net/identity/overview/getting-started/introduction-to-aspnet-identity).

[Read the full tutorial here!](http://www.twilio.com/docs/howto/walkthrough/two-factor-authentication/csharp/mvc)

## Local Development

This project is built using [ASP.NET MVC](http://www.asp.net/mvc) Framework.

1. First clone this repository and `cd` into it.

   ```shell
   git clone git@github.com:TwilioDevEd/authy2fa-csharp.git
   cd authy2fa-csharp
   ```

1. Rename the sample configuration file and edit it to match your configuration.

   ```shell
   rename Authy2FA.Web\Local.config.example Authy2FA.Web\Local.config
   ```

   You can find your **Authy API Key** in your [Authy Dashboard](https://dashboard.authy.com/).

1. Build the solution.

1. Create database and run migrations.

   Make sure SQL Server is up and running.  
   In Visual Studio, open the following command in the [Package Manager
   Console](https://docs.nuget.org/consume/package-manager-console).

   ```shell
   Update-Database
   ```

1. Run the application.

1. Check it out at [http://localhost:49217](http://localhost:49217).

1. Expose application to the wider internet. To [start using
   ngrok](https://www.twilio.com/blog/2015/09/6-awesome-reasons-to-use-ngrok-when-testing-webhooks.html)
   on our project you'll have to execute the following line in the command
   prompt.

   ```shell
   ngrok http 49217 -host-header="localhost:49217"
   ```

## Meta

* No warranty expressed or implied. Software is as is. Diggity.
* [MIT License](http://www.opensource.org/licenses/mit-license.html)
* Lovingly crafted by Twilio Developer Education.
