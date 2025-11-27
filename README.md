
# URL Shortener Service

This is a URL shortener service built with ASP.NET Core Minimal API. It generates compact short links and provides fast redirects to the original URLs.
---

## Features

* 6-character short-code generation with collision-safe logic
* Fast redirect engine mapped directly on root path
* In-memory storage for zero-latency prototyping
* Clean, Minimal-API architecture optimized for integration

---

## Technologies Used

* ASP.NET Core 9.0
* C# 12
* Minimal APIs
* In-memory concurrency storage (`ConcurrentDictionary`)
* **Scalar** – API exploration & contract visualization

---

## Testing / Tooling

* Scalar
* Postman / Insomnia

---

## Installation

```bash
git clone https://github.com/your-username/url-shortener.git
cd url-shortener
dotnet run
```

The service runs on `http://localhost:5000` or `https://localhost:5001`.

---

## API Endpoints

### Generate Short URL

`POST /generate`

Body:

```json
{
  "destinationUrl": "https://example.com"
}
```

Sample response:

```json
{
  "Id": "e4f1c2b7-8a1f-4d9b-9f2e-1c3a5b6d7e8f",
  "DestinationUrl": "https://example.com",
  "ShortenCode": "abc123",
  "CreatedOn": "2025-11-27T21:30:00Z"
}
```

### Redirect

`GET /{shortCode}`

Resolves and redirects to original URL.
Returns `404` if not found.

---

## Usage Flow

1. Create a short link via `/generate`.
2. Retrieve the generated short code.
3. Hit `/{shortCode}` → automatic redirect.

---

## Notes / Future Roadmap

* Current storage is volatile (in-memory).
* Short codes are case-sensitive.
* Expansion runway:

 
The service is positioned for incremental hardening and enterprise alignment:

* Persistent data layer (SQL / MongoDB)
* Observability suite: metrics, counters, request throughput, redirect success rate
* Monitoring hooks (Prometheus-friendly endpoint)
* Full automated test harness: unit tests, integration tests
* Rate-limiting + API governance policies
* Multi-tenant link management
* Analytics dashboard for link performance
---


