# CineTrack — Frontend

Angular 21 SPA (Single Page Application) for CineTrack. Built with standalone components, Angular Signals for reactive state, Tailwind CSS v4, and Chart.js for data visualization. Supports English and Portuguese (BR).

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | Angular 21 (standalone components, no NgModules) |
| State | Angular Signals (`signal`, `computed`) |
| HTTP | Angular `HttpClient` + functional `authInterceptor` |
| Styling | Tailwind CSS v4 + custom SCSS |
| Charts | Chart.js 4 |
| Testing | Vitest |
| i18n | Custom built-in service (EN / PT-BR, zero dependencies) |

## Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── app.ts                    # Root component
│   │   ├── app.config.ts             # provideRouter, provideHttpClient
│   │   ├── app.routes.ts             # All application routes
│   │   ├── core/
│   │   │   ├── guards/
│   │   │   │   └── auth.guard.ts     # authGuard + guestGuard (functional)
│   │   │   ├── interceptors/
│   │   │   │   └── auth.interceptor.ts  # Attaches Bearer token; auto-refreshes on 401
│   │   │   ├── models/
│   │   │   │   ├── auth.model.ts
│   │   │   │   ├── media.model.ts
│   │   │   │   └── achievement.model.ts
│   │   │   └── services/
│   │   │       ├── auth.service.ts       # Login, register, refresh, profile; Signal-based currentUser
│   │   │       ├── media.service.ts      # Catalog queries + personal list CRUD
│   │   │       ├── stats.service.ts      # GET /api/stats
│   │   │       ├── achievement.service.ts # GET /api/achievements
│   │   │       └── i18n.service.ts       # EN / PT-BR translation service
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   │   ├── login/            # LoginComponent
│   │   │   │   └── register/         # RegisterComponent
│   │   │   ├── catalog/
│   │   │   │   ├── catalog.component.*   # Shared for /movies, /series, /anime
│   │   │   │   └── components/media-card/  # MediaCardComponent
│   │   │   ├── dashboard/            # DashboardComponent (stats + charts + lists)
│   │   │   ├── media-detail/         # MediaDetailComponent (/media/:id)
│   │   │   ├── achievements/         # AchievementsComponent
│   │   │   └── profile/              # ProfileComponent
│   │   └── layout/
│   │       ├── main-layout/          # Authenticated shell (sidebar + router-outlet)
│   │       ├── sidebar/              # Nav, theme toggle, language toggle, logout
│   │       └── topbar/               # Top bar
│   ├── environments/
│   │   ├── environment.ts            # apiUrl: http://localhost:5240/api
│   │   └── environment.prod.ts       # apiUrl: Railway production URL
│   ├── index.html
│   └── styles.scss                   # Global styles + Tailwind imports
├── angular.json
├── package.json
├── tsconfig.json
└── tsconfig.app.json
```

## Routes

| Path | Component | Guard |
|---|---|---|
| `/login` | `LoginComponent` | `guestGuard` |
| `/register` | `RegisterComponent` | `guestGuard` |
| `/dashboard` | `DashboardComponent` | `authGuard` |
| `/movies` | `CatalogComponent` (type: Movie) | `authGuard` |
| `/series` | `CatalogComponent` (type: Series) | `authGuard` |
| `/anime` | `CatalogComponent` (type: Anime) | `authGuard` |
| `/achievements` | `AchievementsComponent` | `authGuard` |
| `/profile` | `ProfileComponent` | `authGuard` |
| `/media/:id` | `MediaDetailComponent` | `authGuard` |
| `**` | → `/login` | — |

## Prerequisites

- [Node.js 22+](https://nodejs.org/) and npm

## Local Setup

```bash
cd frontend
npm install
npm start
# App: http://localhost:4200
# Expects the backend running at http://localhost:5240
```

The API URL is configured in `src/environments/environment.ts`. Change it there if your backend runs on a different port.

## Available Scripts

| Command | Description |
|---|---|
| `npm start` | Start dev server (`ng serve`) |
| `npm run build` | Production build (`ng build --configuration production`) |
| `npm test` | Run unit tests with Vitest |
| `npm run watch` | Build in watch mode |

## Key Architecture Patterns

### Angular Signals

All reactive state uses Angular Signals. Services expose `signal()` and `computed()` values rather than RxJS `BehaviorSubject`. Example in `AuthService`:

```typescript
private _currentUser = signal<UserDto | null>(null);
readonly currentUser = this._currentUser.asReadonly();
readonly isAuthenticated = computed(() => this._currentUser() !== null);
```

### Auth Interceptor

The `authInterceptor` in `core/interceptors/auth.interceptor.ts`:
1. Attaches `Authorization: Bearer <accessToken>` to every outgoing request.
2. On a **401** response, automatically calls `POST /api/auth/refresh`.
3. Retries the original request with the new token.
4. Falls back to `logout()` if the refresh also fails.

### i18n Service

`I18nService` is a zero-dependency translation service that stores the selected language in `localStorage`. Toggle between **English** and **Portuguese (BR)** via the sidebar button. All UI labels are sourced from a typed dictionary inside the service.

### Catalog Component

`CatalogComponent` is reused across `/movies`, `/series`, and `/anime` routes. The active `MediaType` is injected via Angular route data (`data: { type: 'Movie' }`), so a single component handles all three content types with filtering, pagination, and status management.

## Production Build

```bash
npm run build
# Output in dist/cinetrack/browser/
```

Configure `src/environments/environment.prod.ts` with the Railway (or other) backend URL before building for production.
