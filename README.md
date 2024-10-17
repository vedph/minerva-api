# Minerva Backend API

This solution contains the backend API for the Minerva web application.

🐋 Quick Docker image build:

```bash
docker build . --build-arg BUILDPLATFORM=linux -t vedph2020/minerva-api:0.0.4 -t vedph2020/minerva-api:latest
docker build . --build-arg BUILDPLATFORM=arm64 -t vedph2020/minerva-api_arm:0.0.1 -t vedph2020/minerva-api_arm:latest
```

(replace with the current version).

Note that these API are not protected by authentication. Anyway they rely on two main technologies to limit the risk of attacks:

- CORS, allowing only the web app to use the API.
- rate limiting, which can be configured via `appsettings.json` and limits the requests per time-window.

## History

### 0.0.4

Added `ASPNETCORE_URLS` to the Dockerfile to ensure that the correct port is used. This is because ASP.NET 8 uses by default 8080 for HTTP (and 8081 for HTTPS), but when run locally it uses 5000 (and 5001 for HTTPS). So, setting the environment variable in the Dockerfile ensures that the correct port is used also in the container.
