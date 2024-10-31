# API Configuration

You can override the API configuration in `appsettings.json` using environment variables. Typically this happens in your `docker-compose.yml` file. The essential guidelines for this are:

- use all uppercase names.
- use `__` (_double_ underscore) as the properties separator (which is represented by `:` in this page).
- for array items, use syntax `__` (=the properties separator, as above) followed by the item's index in the array (zero-based). For instance, `ALLOWEDORIGINS__0` to override the first item in the array.

In the following list of settings, the following icons are used:

- ⚠️ to represent a setting which must be overridden for security reasons.
- 💡 to represent a setting which is typically overridden to adapt the configuration to the hosting environment.

## Database

- 💡 `ConnectionStrings:Default`: the connection string template to the database. There, the database name is replaced by a `{0}` placeholder. Typically you will override this to match your host name and port number.
- `DatabaseName`: the name of the database. This is used to fill the placeholder in the connection string, and to manage the database when seeding is required.
- 💡 `SeedDelay`: the number of seconds to wait before starting the database seed process, when required. When not developing, set a delay here (e.g. 10 seconds or more) so that we can be sure the underlying Postgres service has completed its initialization before seeding.

## Security

### CORS

- ⚠️ `AllowedOrigins`: an array of URLs allowed as CORS clients. You must replace the default item at 0 with your app's URL.

### Rate Limiting

- `RateLimit:IsDisabled`: true to disable rate limiting. Default is false.
- `RateLimit:PermitLimit`: the number of requests allowed in the time window. Default is 20.
- `RateLimit:QueueLimit`: the number of requests that can be queued before the rate limit is enforced. Default is 0.
- `RateLimit:TimeWindow`: the time window in format `HH:MM:SS`. Default is `00:01:00` (1 minute).

## Diagnostics

Use the `Serilog` section to configure logging in detail with Serilog. Refer to <https://github.com/serilog/serilog-settings-configuration> for more information.

- `HasTestEndpoints`: set to `true` to enable test endpoints in the API. Default is `false`. These endpoints are used to test some infrastructure functions of the API. Keep them disabled unless diagnostic data are required.

Test endpoints are:

- `POST api/test/log`: add a diagnostic log entry.
- `GET api/test/exception`: raise an exception to test exception handling (typically in logging).
- `GET api/test/email`: send a test email message to the recipient defined in the configuration (`Mailer:TestRecipient`).

## Messaging

Messaging can be optionally enabled to allow administrators get notifications from the app.

These settings refer to the message content:

- `Messaging:AppName`: the name of the application.
- 💡 `Messaging:ApiRootUrl`: the API root URL. You should change this to the URL of your API.
- 💡 `Messaging:AppRootUrl`: the app root URL. You should change this to the URL of your web app client.
- 💡 `Messaging:SupportEmail`: the email address to use when replying to the message. You can change this to some real address, even if recipients are not supposed to reply to notification messages.

These settings refer to the mailing service used to send messages:

- 💡 `Mailer:IsEnabled`: true to enable sending messages. Default is `false`.
- 💡 `Mailer:SenderEmail`: the email address to be displayed as the message's sender.
- `Mailer:SenderName`: the name of the sender to be displayed.
- 💡 `Mailer:Host`: the SMTP host address.
- 💡 `Mailer:Port`: the SMTP host port.
- `Mailer:UseSsl`: `true` to use SSL (default).
- 💡 `Mailer:UserName`: the SMTP host username.
- 💡 `Mailer:Password`: the SMTP host password.
- 💡 `Mailer:TestRecipient`: the API test endpoints recipient email address. A specific test endpoint is used to test messaging, and targets this recipient. Set it to your email as an admin, so that you can receive the test message.
- 💡 `Mailer:Recipients`: the recipient email addres(ses) to send a message to when rate limit is exceeded. Set here the recipients who are in charge to handle these notifications.
