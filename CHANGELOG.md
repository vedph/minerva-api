# History

## 4.0.0

- 2026-11-14: ⚠️ upgraded to .NET 11. Note that Entity Framework Core related packages are not yet updated as we wait for the PostgreSQL provider to be released.

## 3.0.0

- 2026-10-22: ⚠️ updated packages (Pythia v13).

## 2.1.0

- 2026-10-16:
  - ⚠️ updated packages to Pythia 12.1.2 (required, as these changes depend on it).
  - ⚠️ the API no longer serves the full text of documents, as it is public: new `Reader:IsFullTextDisabled` setting (env `READER__ISFULLTEXTDISABLED`), **true by default**. When true, `documents/{id}/text`, `documents/{id}/map` and `documents/{id}/path/{path}` return 403; `documents/{id}?content=true` returns no content; `documents/{id}/range/{start}/{end}` returns 403 when the context would include the whole document map (e.g. any range wider than the map). Set it to false only for an API used by teammates, who can read whole documents.
  - ⚠️ search CSV export is limited to `SearchExport:MaxRows` results per request (env `SEARCHEXPORT__MAXROWS`, default 10000; 0=unlimited). The frontend exports all the results, so that a single anonymous request for a very frequent word loaded the database for a long time (and provided the contexts of all its occurrences).
  - profiles are returned without their content (the indexing configuration), which is not used by the frontend.
  - `ReaderController` and `SearchController` were updated with the fixes of the corresponding Pythia controllers:
    - reading a search result inside the last node of a document map (e.g. the last paragraph) returned the whole document text instead of its context.
    - reading documents having several attributes with the same name failed with a 500 error; a missing text or map now returns 404 rather than 500; contexts spanning nodes on different map levels were not well-formed (500).
    - document maps were cached forever; they now expire after 30 minutes of inactivity.
    - CSV export: the export task was not awaited, so that the response could be closed while still writing; the export now uses the same literal filters and sort fields of the search; search results contexts are fetched in batches (as in Pythia) to avoid PostgreSQL shared memory errors; search errors are logged.
  - added forwarded headers support (`X-Forwarded-For`, `X-Forwarded-Proto`), so that behind a reverse proxy the rate limiter partitions requests by client IP address rather than by the proxy's one. Besides localhost, only the proxies listed in `ForwardedHeaders:KnownProxies` are trusted (e.g. env `FORWARDEDHEADERS__KNOWNPROXIES__0=172.18.0.5`).
  - removed the unused JWT settings.
  - besides single proxies, trusted proxy networks can be set in `ForwardedHeaders:KnownNetworks` (CIDR, e.g. env `FORWARDEDHEADERS__KNOWNNETWORKS__0=172.16.0.0/12`), which is required when a reverse proxy in the Docker host forwards requests to the API container.
  - search errors: only query errors (like syntax errors) are returned to users with their message; any other error (e.g. from the database) is logged, and users just get a generic message.
  - the startup dump of environment variables no longer logs secrets, like the database password in the connection string.
  - the rate limiter rejection response is marked as JSON and has a `Retry-After` header; its partition key is logged as debug rather than information; rejections log the client key and path; the rate limit exceeded email notification is sent at most once every 30 minutes, rather than once for each rejected request (which also delayed the 429 response).
  - README: added a guide for server settings, including rate limiting behind a reverse proxy.

## 2.0.0

- 2026-08-31: updated packages.
- 2026-07-10: updated packages.

## 1.1.6

- 2026-06-26:
  - updated packages.
  - refactored Docker scripts.
- 2026-05-27: updated packages.
- 2026-02-24: updated packages.

## 1.1.5

- 2026-01-19:
  - updated packages.
  - new Dockerfile for MacOS support.
- 2026-01-09: updated packages.
- 2025-12-17: updated packages.
- 2025-11-24: ⚠️ upgraded to NET 10.
- 2025-09-29: updated packages.
- 2025-09-18: updated packages.
- 2025-09-08: updated packages.

## 1.1.3

- 2025-07-29: updated packages.
- 2025-06-15: updated packages.

## 1.1.2

- 2025-02-11: updated packages.
- 2025-02-09: updated packages.

## 1.1.1

- 2025-02-08: updated packages (fixes in highlighter and indexing).

## 1.1.0

- 2025-02-07: updated packages (added POS filter to word index).

## 1.0.2

- 2025-01-29:
  - updated Pythia packages (fix to `XmlHighlighter`).
  - disable rate limiter in app settings and avoid its middleware when disabled.

## 1.0.1

- 2025-01-29: updated packages.
- 2025-01-22: updated packages.
- 2025-01-20: updated packages.
- 2025-01-09: updated packages.
- 2024-12-29: updated packages.
- 2024-12-20:
  - replaced Swagger with Scalar. 
  - updated packages.
- 2024-11-22: updated packages.

## 1.0.0

- 2024-11-13: ⚠️ upgraded to .NET 9.

## 0.0.8

- 2024-11-10: updated packages.

## 0.0.7

- 2024-11-06: rate limiting configuration.

## 0.0.6

- 2024-11-04: commented out writable endpoints.
- 2024-10-31: better logging configuration.
- 2024-10-30: added test controllers.

## 0.0.5

- 2024-10-25: new Docker image.
- 2024-10-18:
  - updated packages.
  - fixes to rate limiter.

## 0.0.4

- 2024-10-17: added `ASPNETCORE_URLS` to the Dockerfile to ensure that the correct port is used. This is because ASP.NET 8 uses by default 8080 for HTTP (and 8081 for HTTPS), but when run locally it uses 5000 (and 5001 for HTTPS). So, setting the environment variable in the Dockerfile ensures that the correct port is used also in the container.
