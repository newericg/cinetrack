# CineTrack — Backend

ASP.NET Core Web API (.NET 10) serving as the REST backend for CineTrack. Handles authentication, the global media catalog, per-user watchlists, statistics, and achievements — all backed by MongoDB Atlas.

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| Database | MongoDB Atlas (via `MongoDB.Driver` 3.7) |
| Authentication | JWT Bearer + rotating Refresh Tokens |
| Password hashing | BCrypt (`BCrypt.Net-Next`) |
| External API | [Jikan](https://jikan.moe/) (MyAnimeList, for anime poster images during seeding) |
| API docs | Scalar / OpenAPI (development only) |

## Project Structure

```
backend/
├── Controllers/
│   ├── AuthController.cs          # POST /register, /login, /refresh, /logout; GET|PUT /me
│   ├── CatalogController.cs       # GET /catalog (paged, filtered), /catalog/genres, /catalog/:id
│   ├── MediaController.cs         # GET|POST /media, DELETE|PATCH /media/:id/...
│   ├── StatsController.cs         # GET /stats
│   ├── AchievementsController.cs  # GET /achievements
│   └── DevController.cs           # POST /dev/seed  (dev-only)
├── DTOs/
│   ├── AuthDtos.cs
│   ├── MediaDtos.cs
│   ├── AchievementDtos.cs
│   └── StatsDtos.cs
├── Models/
│   ├── CatalogItem.cs             # Global catalog document
│   ├── UserMediaItem.cs           # Per-user watchlist entry
│   ├── User.cs                    # User account
│   └── MediaItem.cs               # Shared enums (MediaType, WatchStatus)
├── Services/
│   ├── MongoDbService.cs          # IMongoDatabase wrapper
│   ├── AuthService.cs             # Registration, login, profile
│   ├── TokenService.cs            # JWT + refresh token generation/validation
│   ├── CatalogService.cs          # Global catalog queries + user overlays
│   ├── UserMediaService.cs        # Personal watchlist CRUD
│   ├── StatsService.cs            # Aggregated statistics
│   ├── AchievementService.cs      # Achievement evaluation (15 achievements)
│   ├── AchievementDefinition.cs   # Achievement definition record
│   ├── JikanService.cs            # Anime poster fetching (Jikan / MyAnimeList)
│   └── DataSeeder.cs              # Startup seed for catalog (movies, series, anime)
├── Settings/
│   ├── MongoDbSettings.cs
│   └── JwtSettings.cs
├── Program.cs
├── appsettings.json               # Production defaults (no secrets)
├── appsettings.Development.json   # Dev overrides (gitignored — contains real connection string)
└── CineTrack.API.csproj
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A [MongoDB Atlas](https://www.mongodb.com/atlas) cluster (free M0 tier is sufficient)

## Local Setup

### 1. Clone and restore

```bash
cd backend
dotnet restore
```

### 2. Configure `appsettings.Development.json`

This file is **gitignored**. Create it (or restore from your secrets manager) with the following structure:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://<user>:<password>@<cluster>.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "cinetrack-dev"
  },
  "JwtSettings": {
    "Secret": "<a-long-random-secret-256-bits>",
    "Issuer": "CineTrackAPI",
    "Audience": "CineTrackClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3. Run

```bash
dotnet run
# API: http://localhost:5240
# OpenAPI UI (dev): http://localhost:5240/openapi
```

On first startup, `DataSeeder` automatically populates the `catalog` collection with ~50 movies, series, and anime titles. Anime poster images are fetched from the Jikan API on seeding.

## API Reference

All routes are prefixed with `/api`.

### Auth — `api/auth`

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/register` | — | Create account |
| POST | `/login` | — | Returns `accessToken` + `refreshToken` |
| POST | `/refresh` | — | Rotate refresh token |
| POST | `/logout` | Bearer | Revoke refresh token |
| GET | `/me` | Bearer | Get current user profile |
| PUT | `/me` | Bearer | Update profile (name, avatar, timezone) |

### Catalog — `api/catalog`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/` | Optional Bearer | Paged catalog with filters (`type`, `genre`, `search`, `sort`, `page`, `pageSize`). When authenticated, merges the user's personal status overlay. |
| GET | `/genres` | — | List of all genres in the catalog |
| GET | `/:id` | Optional Bearer | Single catalog item |

### User List — `api/media`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/` | Bearer | User's personal list |
| POST | `/` | Bearer | Add catalog item to list |
| DELETE | `/:id` | Bearer | Remove item from list |
| PATCH | `/:id/watched` | Bearer | Toggle watched flag |
| PATCH | `/:id/rating` | Bearer | Set user rating (1–10) |
| PATCH | `/:id/episodes` | Bearer | Set episodes watched count |

### Stats — `api/stats`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/` | Bearer | Total counts, hours watched, avg rating, top genres, monthly breakdown |

### Achievements — `api/achievements`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/` | Bearer | All 15 achievements with progress and locked/unlocked state |

## Authentication Flow

1. `POST /api/auth/login` → `{ accessToken, refreshToken, user }`
2. Frontend attaches `Authorization: Bearer <accessToken>` to all requests.
3. Access token expires in 60 min (dev) / 15 min (prod).
4. The Angular `authInterceptor` catches **401** responses, calls `POST /api/auth/refresh` automatically, and retries the original request.
5. On logout, the refresh token is revoked in MongoDB.

## Data Model

### `catalog` collection — `CatalogItem`

Global, shared across all users. Seeded on startup.

```
_id, title, type (Movie|Series|Anime), genres[], year, posterUrl,
backdropUrl, synopsis, rating, durationMinutes, totalEpisodes,
director, writers[], studio, cast[{ name, character }], trailerUrl
```

### `userMediaItems` collection — `UserMediaItem`

One document per (user, catalog item) pair.

```
_id, userId, catalogItemId, status (ToWatch|Watching|Watched|Dropped),
userRating, episodesWatched, isWatched, watchedAt, addedAt
```

### `users` collection — `User`

```
_id, name, email, passwordHash, avatarUrl, timezone,
refreshToken, refreshTokenExpiry
```

## Environment Variables (Production)

For Railway or any cloud deployment, set these environment variables (they override `appsettings.json`):

```
MongoDbSettings__ConnectionString=<atlas-connection-string>
MongoDbSettings__DatabaseName=cinetrack-prod
JwtSettings__Secret=<strong-random-secret>
JwtSettings__AccessTokenExpirationMinutes=15
JwtSettings__RefreshTokenExpirationDays=7
```
