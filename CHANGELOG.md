# History

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
