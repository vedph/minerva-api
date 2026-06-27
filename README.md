# Minerva Backend API

This solution contains the backend API for the Minerva web application.

🐋 Quick Docker image build:

```bash
docker buildx create --use

docker buildx build . --platform linux/amd64,linux/arm64,windows/amd64 -t vedph2020/minerva.api:1.1.6 -t vedph2020/minerva.api:latest --push
```

(replace with the current version).

Note that these API are not protected by authentication. Anyway they rely on two main technologies to limit the risk of attacks:

- CORS, allowing only the web app to use the API.
- rate limiting, which can be configured via `appsettings.json` and limits the requests per time-window. Parameters:
  - `IsDisabled`: if `true`, the rate limiting is disabled. Default is `true`.
  - `PermitLimit`: the number of requests allowed in the time window. Default is 100.
  - `QueueLimit`: the number of requests that can be queued before the rate limit is enforced. Default is 0.
  - `TimeWindow`: the time window in format `HH:MM:SS`. Default is `00:01:00` (1 minute).

To locally test messaging you can use [MailSlurper](https://github.com/mailslurper/mailslurper):

```bash
# Build container image (adjust repo location as necessary)
docker build -t mailslurper 'https://github.com/mailslurper/mailslurper#master'

# Run a temporary container. Note that upon shutdown, all stored messages will be lost when using this config.
docker run -it --rm --name mailslurper -p 8080:8080 -p 8085:8085 -p 2500:2500 mailslurper
```
