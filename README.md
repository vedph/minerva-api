# Minerva Backend API

This solution contains the backend API for the Minerva web application.

🐋 Quick Docker image build:

```bash
docker build . --build-arg BUILDPLATFORM=linux -t vedph2020/minerva-api:1.0.0 -t vedph2020/minerva-api:latest
docker build . --build-arg BUILDPLATFORM=arm64 -t vedph2020/minerva-api_arm:1.0.0 -t vedph2020/minerva-api_arm:latest
```

(replace with the current version).

Note that these API are not protected by authentication. Anyway they rely on two main technologies to limit the risk of attacks:

- CORS, allowing only the web app to use the API.
- rate limiting, which can be configured via `appsettings.json` and limits the requests per time-window. Parameters:
  - `IsDisabled`: if `true`, the rate limiting is disabled. Default is `false`.
  - `PermitLimit`: the number of requests allowed in the time window. Default is 20.
  - `QueueLimit`: the number of requests that can be queued before the rate limit is enforced. Default is 0.
  - `TimeWindow`: the time window in format `HH:MM:SS`. Default is `00:01:00` (1 minute).

To locally test messaging you can use [MailSlurper](https://github.com/mailslurper/mailslurper):

```bash
# Build container image (adjust repo location as necessary)
docker build -t mailslurper 'https://github.com/mailslurper/mailslurper#master'

# Run a temporary container. Note that upon shutdown, all stored messages will be lost when using this config.
docker run -it --rm --name mailslurper -p 8080:8080 -p 8085:8085 -p 2500:2500 mailslurper
```

## History

- 2024-12-29: updated packages.
- 2024-12-20:
  - replaced Swagger with Scalar. 
  - updated packages.
- 2024-11-22: updated packages.

### 1.0.0

- 2024-11-13: ⚠️ upgraded to .NET 9.

### 0.0.8

- 2024-11-10: updated packages.

### 0.0.7

- 2024-11-06: rate limiting configuration.

### 0.0.6

- 2024-11-04: commented out writable endpoints.
- 2024-10-31: better logging configuration.
- 2024-10-30: added test controllers.

### 0.0.5

- 2024-10-25: new Docker image.
- 2024-10-18:
  - updated packages.
  - fixes to rate limiter.

### 0.0.4

- 2024-10-17: added `ASPNETCORE_URLS` to the Dockerfile to ensure that the correct port is used. This is because ASP.NET 8 uses by default 8080 for HTTP (and 8081 for HTTPS), but when run locally it uses 5000 (and 5001 for HTTPS). So, setting the environment variable in the Dockerfile ensures that the correct port is used also in the container.
