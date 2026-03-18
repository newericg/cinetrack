# CineTrack

A full-stack media tracking application for movies, TV series, and anime. Users can manage personal watchlists, track watch progress, rate titles, view statistics, and unlock achievements.

## Tech Stack

| Layer | Technology |
|---|---|
| **Frontend** | Angular 21 (Standalone Components, Signals) |
| **Backend** | ASP.NET Core Web API (.NET 10) |
| **Database** | MongoDB Atlas |
| **Auth** | JWT (access token) + Refresh Token (rotating) |
| **Styling** | Tailwind CSS v4 + custom SCSS |
| **Charts** | Chart.js 4 |

## Project Structure

```
CineTrack/
├── backend/        # ASP.NET Core Web API
└── frontend/       # Angular SPA
```

## Features

- **Catalog** — Browse a shared catalog of movies, series, and anime with search, genre filters, and status overlays
- **Personal List** — Add titles to your list, mark as watched/watching/dropped, and rate them (1–10)
- **Episode Tracking** — Track progress for series and anime episode by episode
- **Statistics** — Overview cards + Chart.js charts (genre distribution, monthly watch time)
- **Achievements** — 15 achievements with progress tracking across movies, series, and anime
- **Internationalization** — Full English / Portuguese (BR) toggle via a built-in i18n service
- **Dark / Light theme** — Persisted in local storage

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and npm
- A [MongoDB Atlas](https://www.mongodb.com/atlas) cluster (free tier works)

### Backend

See [`backend/README.md`](./backend/README.md) for full setup instructions.

```bash
cd backend
dotnet restore
dotnet run
# API available at http://localhost:5240
```

### Frontend

See [`frontend/README.md`](./frontend/README.md) for full setup instructions.

```bash
cd frontend
npm install
npm start
# App available at http://localhost:4200
```

## Architecture Overview

```
Angular SPA (port 4200)
        │
        │  HTTP/JSON  (JWT Bearer)
        ▼
ASP.NET Core Web API (port 5240)
        │
        │  MongoDB Driver
        ▼
MongoDB Atlas
  ├── catalog          (global media catalog)
  ├── users            (accounts + refresh tokens)
  └── userMediaItems   (per-user watchlist entries)
```

The catalog and user data are stored in separate collections. The `CatalogService` merges global catalog documents with each user's personal overlay (status, rating, episode progress) at query time, producing a unified response to the frontend.

## Deployment

- **Backend** — configured for [Railway](https://railway.app/) (see `environment.prod.ts` for the API URL placeholder)
- **Frontend** — standard `ng build --configuration production` output, deployable to any static host (Vercel, Netlify, Railway)
