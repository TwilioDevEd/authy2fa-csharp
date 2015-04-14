# Two-Factor Authentication with Authy in ASP.NET MVC

This example application demonstrates how to use [Authy](http://www.authy.com) as the two-factor authentication provider in an ASP.NET MVC project using [ASP.NET Identity](http://www.asp.net/identity/overview/getting-started/introduction-to-aspnet-identity).

For more information on how this code works, [check out our interactive code walkthrough](http://www.twilio.com/docs/howto/walkthrough/two-factor-authentication/csharp/mvc#1).

## Running Locally

In order to run this project locally, you will need to move/rename `Authy2FA\Authy2FA\Web.config.sample` to `Authy2FA\Authy2FA\Web.config`. In this file, you will need to locate and change this value to your Authy production key from your dashboard:

        
```xml
<add key="AuthyKey" value="your authy production key"/>
```

You might also need to run through the initial set of migrations for Entity Framework. In the NuGet Package Manager console, enter:

```bash
Create-Database
```

## License

MIT