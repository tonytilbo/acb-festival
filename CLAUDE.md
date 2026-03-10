# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Start local storage emulator (required for API)
azurite --silent

# Run API (Terminal 2)
make api        # cd src/api && dotnet run --launch-profile http

# Run UI dev server (Terminal 3)
make ui         # cd src/ui && npm run dev

# UI scripts (run from src/ui/)
npm run build        # type-check + vite build
npm run type-check   # vue-tsc --build
npm run lint         # oxlint then eslint (both with --fix)
npm run format       # prettier --write src/
```

## Architecture

**Stack:** Vue 3 + TypeScript frontend · .NET 10 Minimal API backend · Azure Table Storage

**Production:**
```
User → Azure Static Web Apps → (proxies /api/*) → Azure Container Apps → .NET API → Azure Table Storage
```

**Local:** API at `http://localhost:5173` (proxied via Vite), Azurite as the storage emulator.

### Backend (`src/api/Program.cs`)

Single-file Minimal API. All logic lives in `Program.cs`:
- Beer data is hardcoded in `BeerData.All` (static array of `Beer` records)
- Ratings stored in Azure Table Storage (`Ratings` table), partitioned by `userId`, row key is `beerId`
- Auth: users get a UUID from `POST /api/register`; the UUID is stored in a cookie client-side (`acb_user_id`) for 1 year
- In production uses `DefaultAzureCredential` (Managed Identity); locally falls back to Azurite via `UseDevelopmentStorage=true`

**API routes:** `POST /api/register`, `GET /api/beers`, `GET /api/ratings`, `POST /api/ratings`, `DELETE /api/ratings`, `GET /api/results`, `GET /api/results/{beerId}`

### Frontend (`src/ui/src/`)

- **Pinia stores:** `user.ts` (registration + userId cookie), `ratings.ts` (fetch/submit/clear ratings, keyed by beerId), `beers.ts` (beer list from API)
- **Views:** `HomeView` (landing/register), `BeersView` (beer list with rating UI), `ResultsView` (aggregate scores)
- **Components:** `BeerCard` (displays beer + rating), `RatingPicker` (1–10 selector), `ScrollFadeItem` (scroll animation)
- All API calls use relative paths (`/api/...`) — Vite proxies to the API in dev, Static Web Apps proxies in prod

### Infrastructure (`infra/`)

Terraform provisions: Azure Static Web Apps, Azure Container Apps, Azure Container Registry, Azure Table Storage.
