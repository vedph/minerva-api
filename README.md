# Minerva Backend API

This solution contains the backend API for the Minerva web application.

🐋 Quick Docker image build:

🐋 Before creating Docker images, ensure you have a buildx builder instance running that supports multi-arch and that you have run `buildnpub.ps1` publishing NuGet packages:

```sh
docker buildx create --use --name multi-arch-builder || docker buildx use multi-arch-builder
docker buildx inspect --bootstrap
```

> To run natively on Linux VMs, macOS (both Intel and Apple Silicon), and Windows (via WSL2 or Docker Desktop)—`linux/amd64` and `linux/arm64` are the only two targets we need. Note that `docker buildx` automatically injects variables like `TARGETARCH` and `TARGETOS` into the scope of your build. In `Dockerfile` we pass these directly to the .NET CLI commands.

These commands build for multiple platforms and push directly to Docker Hub:

- 🐋 **Minerva.Api**:

```sh
docker buildx build --platform linux/amd64,linux/arm64 -t vedph2020/minerva-api:2.1.0 -t vedph2020/minerva-api:latest --push .
```

> When there is no available ARM image, add `platform: linux/amd64` as a sibling of the `image` line in docker compose script to force Docker to pull and run the x64 image under emulation, e.g.:

- add `platform: linux/arm64` to MongoDB and PostgreSQL. Note that some images (especially for PostgreSQL) are not available for ARM, so you may need to add `platform: linux/amd64` to them too or downgrade to a version which supports ARM (e.g. `image: postgres:16`).
- add `platform: linux/amd64` to API and app.

## Server Settings

This API is public and has no authentication, and it is read-only (no write endpoints). In a server, set these options in `appsettings.json` or, more commonly, via environment variables in `docker-compose.yml` (a `:` in a setting path becomes `__`, and array items are numbered, e.g. `ForwardedHeaders:KnownNetworks[0]` is `FORWARDEDHEADERS__KNOWNNETWORKS__0`):

| setting | environment variable | default | description |
| --- | --- | --- | --- |
| `AllowedOrigins` | `ALLOWEDORIGINS__0`, ... | | the origins of the web app (CORS). |
| `Reader:IsFullTextDisabled` | `READER__ISFULLTEXTDISABLED` | `true` | never serve whole documents, but only the context of search results. Set to `false` only for the teammates API. |
| `SearchExport:MaxRows` | `SEARCHEXPORT__MAXROWS` | `10000` | the maximum number of rows exported to CSV by a single request (0=unlimited). |
| `ForwardedHeaders:KnownProxies` | `FORWARDEDHEADERS__KNOWNPROXIES__0` | | the IP addresses of the trusted reverse proxies (see below). |
| `ForwardedHeaders:KnownNetworks` | `FORWARDEDHEADERS__KNOWNNETWORKS__0` | | the networks (CIDR, e.g. `172.16.0.0/12`) of the trusted reverse proxies (see below). |
| `RateLimit:IsDisabled` | `RATELIMIT__ISDISABLED` | `true` | true to disable rate limiting. |
| `RateLimit:PermitLimit` | `RATELIMIT__PERMITLIMIT` | `100` | the number of requests allowed to each client in the time window. |
| `RateLimit:QueueLimit` | `RATELIMIT__QUEUELIMIT` | `0` | the number of requests over the limit which are queued (rather than rejected) until the next time window. |
| `RateLimit:TimeWindow` | `RATELIMIT__TIMEWINDOW` | `00:01:00` | the time window, as `HH:MM:SS`. |

Note that CORS only prevents other websites from using the API from their pages in a browser; it does not stop other kinds of clients, so the real protections are the read-only API, the full text restriction, the export limit, and rate limiting.

### Rate Limiting

When enabled, the rate limiter allows each client up to `PermitLimit` requests per `TimeWindow`; further requests get a 429 (Too Many Requests) response with a `Retry-After` header, until the window ends. Clients are identified by their **IP address**, so the limit only works as expected when the API sees the real client IP address. This is the most common cause of unexpected limit hits: when all the requests seem to come from the same address, all the users share a single quota, which is quickly exhausted.

In Docker this happens when:

- a **reverse proxy** (in Docker, like nginx-proxy, or in the host, like NGINX or Apache), providing HTTPS, forwards requests to the API container: the API sees the address of the proxy for every request;
- even without a proxy, in some Docker setups (e.g. userland proxy, rootless Docker, Docker Desktop) the API sees the address of the Docker network gateway (like `172.18.0.1`) for every request.

To check, set the `Serilog:MinimumLevel` to `Debug` (the default in `appsettings.json`) and look at the `Rate limit key: ...` lines in the API log while using the app from different machines: each user should have a different key. Rejected requests are logged as warnings (`Rate limit exceeded by ...`).

The recommended setup in a VM is a **dockerized reverse proxy** ([nginx-proxy](https://github.com/nginx-proxy/nginx-proxy) with its LetsEncrypt companion), which provides HTTPS and routes each subdomain to its container via a shared Docker network (e.g. `nginx-proxy`). The proxy already sets the `X-Forwarded-For` header to the client address; the API must just trust it when coming from the proxy network:

1. create the proxy network with a known subnet, e.g. `docker network create --subnet 172.30.0.0/16 nginx-proxy` (for an existing network, get it with `docker network inspect nginx-proxy -f '{{(index .IPAM.Config 0).Subnet}}'`).
2. in the API service of `docker-compose.yml`, trust that network and enable the rate limiter. The proxy reaches the API via the Docker network, so its port needs not be published; if you publish it, bind it to localhost only, so that the API cannot be reached bypassing the proxy:

    ```yml
    minerva-api:
      # optional
      ports:
        - 127.0.0.1:5180:8080
      environment:
        # ...
        - VIRTUAL_HOST=minerva-api.YOURDOMAIN
        - VIRTUAL_PORT=8080
        - LETSENCRYPT_HOST=minerva-api.YOURDOMAIN
        - FORWARDEDHEADERS__KNOWNNETWORKS__0=172.30.0.0/16
        - RATELIMIT__ISDISABLED=false
        - RATELIMIT__PERMITLIMIT=300
        - RATELIMIT__TIMEWINDOW=00:01:00
      networks:
        - nginx-proxy
    ```

The same applies to the teammates API, if any (with its own subdomain). With a reverse proxy installed in the host rather than in Docker (e.g. NGINX with `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;`, or Apache's `mod_proxy`, which sets it automatically), requests come from the Docker network gateway: trust `172.16.0.0/12` (the default Docker networks) or the address shown in the `Rate limit key` log lines. Without any proxy, do not set known networks or proxies: the API would trust forwarded headers sent by anyone.

The frontend app container (NGINX serving static files) is not involved: the browser calls the API directly at the `apiUrl` of `env.js`.

As for the limit value, consider that the app sends several requests for a single user action: e.g. a search sends 1 request per page; reading a result 2 requests; browsing the words index 1-2 requests for each expanded node or page. So, a very active user hardly sends more than 60-100 requests per minute. Yet, all the users behind the same NAT (e.g. a classroom, or a university network) share the same public IP address, and thus the same quota. A limit like 300 requests per minute is a reasonable compromise: it blocks automated scraping (which would anyway take hours for a large corpus) without affecting human users. Keep `QueueLimit` at 0: with a fixed time window, queued requests would just wait until the window ends, making the app look frozen rather than reporting an error.

When the mailer is enabled (`Mailer:IsEnabled`), limit hits are notified by email to `Mailer:Recipients`, at most once every 30 minutes.

To locally test messaging you can use [MailSlurper](https://github.com/mailslurper/mailslurper):

```bash
# Build container image (adjust repo location as necessary)
docker build -t mailslurper 'https://github.com/mailslurper/mailslurper#master'

# Run a temporary container. Note that upon shutdown, all stored messages will be lost when using this config.
docker run -it --rm --name mailslurper -p 8080:8080 -p 8085:8085 -p 2500:2500 mailslurper
```
